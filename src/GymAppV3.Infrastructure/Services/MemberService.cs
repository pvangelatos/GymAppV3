using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Members;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Extensions;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Mapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Transactions;

namespace GymAppV3.Infrastructure.Services;

/// <summary>
/// Service for managing member profiles with authorization-aware operations.
/// </summary>
public class MemberService : IMemberCommandService, IMemberQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _clock;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly int _minimumAge;

    public MemberService(
        ApplicationDbContext context,
        IUserContext userContext,
        IDateTimeProvider clock,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        _context = context;
        _userContext = userContext;
        _clock = clock;
        _userManager = userManager;
        _configuration = configuration;
        _minimumAge = _configuration.GetValue<int>("BusinessRules:MinimumMemberAge", 10);
    }

    #region Command Operations

    public async Task<MemberDto> CreateAsync(CreateMemberCommand command, CancellationToken cancellationToken = default)
    {
        // Offline ledger entries are staff-only
        EnsureIsAdminOrTrainer();

        EnsureMinimumAge(command.BirthDate);

        await EnsureEmailIsFreeAsync(command.Email, null, cancellationToken);

        var member = new Member
        {
            UserId = null,          // oofline member - never bound to an account at creation time
            Firstname = command.Firstname,
            Lastname = command.Lastname,
            Email = command.Email,
            Phone = command.Phone,
            Address = command.Address.ToEntity(),
            BirthDate = command.BirthDate,
            HasMedicalConditions = command.HasMedicalConditions,
            MedicalNotes = command.MedicalNotes
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return member.ToDto();
    }

    public async Task<MemberDto> CompleteProfileAsync(CompleteMemberProfileCommand command, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.UserId ??
            throw new UnauthorizedAccessException("User is not authenticated.");

        // One profile per account — the filtered unique index enforces this at the DB
        // level too, but we want a clean 422 instead of a DbUpdateException.
        var alreadyHasProfile = await _context.Members
            .AnyAsync(m => m.UserId == userId, cancellationToken);

        if (alreadyHasProfile)
            throw new BusinessRuleException("A member profile already exists for this account.");

        EnsureMinimumAge(command.BirthDate);
        await EnsureEmailIsFreeAsync(command.Email, null, cancellationToken);

        var member = new Member
        {
            UserId = userId,   // from the token, not the body
            Firstname = command.Firstname,
            Lastname = command.Lastname,
            Email = command.Email,
            Phone = command.Phone,
            Address = command.Address.ToEntity(),
            BirthDate = command.BirthDate,
            HasMedicalConditions = command.HasMedicalConditions,
            MedicalNotes = command.MedicalNotes
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return member.ToDto();
    }

    public async Task<MemberDto> UpdateAsync(UpdateMemberCommand command, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), command.Id);

        // Authorization check: Member can only update their own profile, Admin can update any
        EnsureCanModify(member);

        // Validate age
        EnsureMinimumAge(command.BirthDate);

        // Check if email is free (excluding the current member)
        var emailChanged = member.Email != command.Email;
        if (emailChanged)
            await EnsureEmailIsFreeAsync(command.Email, member.Id, cancellationToken);

        // UserManager and this service share the same scoped DbContext, so an explicit
        // transaction makes the Identity write and the Member write commit or roll back together.
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (emailChanged && member.UserId is not null)
        {
            var user = await _userManager.FindByIdAsync(member.UserId);
            if (user is not null)
            {
                // These handle normalization through the configured ILookupNormalizer
                // and refresh the security stamp — don't set the Normalized* fields by hand.
                var emailResult = await _userManager.SetEmailAsync(user, command.Email);
                if (!emailResult.Succeeded)
                    throw new BusinessRuleException(
                        $"Could not update the login email: {string.Join("; ", emailResult.Errors.Select(e => e.Description))}");

                var nameResult = await _userManager.SetUserNameAsync(user, command.Email);
                if (!nameResult.Succeeded)
                    throw new BusinessRuleException(
                        $"Could not update the username: {string.Join("; ", nameResult.Errors.Select(e => e.Description))}");
            }
        }

        // Update member fields
        member.Firstname = command.Firstname;
        member.Lastname = command.Lastname;
        member.Email = command.Email;
        member.Phone = command.Phone;
        member.Address = command.Address.ToEntity();
        member.BirthDate = command.BirthDate;
        member.HasMedicalConditions = command.HasMedicalConditions;
        member.MedicalNotes = command.MedicalNotes;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return member.ToDto();
    }

    public async Task DeleteAsync(DeleteMemberCommand command, CancellationToken cancellationToken = default)
    {
        // Check permission before doing the expensive load.
        EnsureIsAdmin();

        var member = await _context.Members
            .Include(m => m.Memberships)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), command.MemberId);

        var now = _clock.UtcNow;
        var currentUserId = _userContext.UserId!;

        // Soft-delete member
        member.IsDeleted = true;
        member.DeletedAt = now;
        member.DeletedBy = currentUserId;

        // Cascade soft-delete to Memberships
        foreach (var membership in member.Memberships)
        {
            membership.IsDeleted = true;
            membership.DeletedAt = now;
            membership.DeletedBy = currentUserId;
        }

        // Cancel active Bookings (not soft-delete, just set status to Cancelled)
        foreach (var booking in member.Bookings.Where(b => b.Status != BookingStatus.Cancelled))
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = now;
        }

        // Payments are preserved (no changes)

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Query Operations

    public async Task<MemberDetailDto?> GetByIdAsync(GetMemberByIdQuery query, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == query.MemberId, cancellationToken);

        if (member == null)
            return null;

        var dto = member.ToDetailDto();

        // Everyone sees HasMedicalConditions, but MedicalNotes is restricted.
        return await CanSeeMedicalNotesAsync(member, cancellationToken)
            ? dto
            : dto with { MedicalNotes = null };
    }

    public async Task<MemberDto?> GetByUserIdAsync(GetMemberByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Members
            .Where(m => m.UserId == query.UserId)
            .Select(ObjectMapper.Member.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ResultSet<MemberDto>> GetAllAsync(GetAllMembersQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view all members
        EnsureIsAdminOrTrainer();

        var membersQuery = _context.Members.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm;
            membersQuery = membersQuery.Where(m =>
                m.Firstname.ToLower().Contains(search) ||
                m.Lastname.ToLower().Contains(search) ||
                m.Email.ToLower().Contains(search));
        }

        // Apply active membership filter
        if (query.HasActiveMembership.HasValue)
        {
            var now = _clock.UtcNow;
            var hasAtive = query.HasActiveMembership.Value;

            membersQuery = membersQuery.Where(m =>
               m.Memberships.Any(ms =>
                    ms.Status == MembershipStatus.Active &&
                    ms.StartDate <= now &&
                    ms.EndDate >= now) == hasAtive);
        }

        // Project before paging: MedicalNotes never leaves the database.
        return await membersQuery
            .Select(ObjectMapper.Member.ToDto)
            .ToResultSetAsync(query.Options, cancellationToken);
    }

    public async Task<ResultSet<MemberDto>> GetByActiveBookingsAsync(GetMembersByActiveBookingsQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view
        EnsureIsAdminOrTrainer();

        var now = _clock.UtcNow;

        return await _context.Members
            .Where(m => m.Bookings.Any(b =>
                b.Status != BookingStatus.Cancelled &&
                b.ClassSession.StartsAt > now))
            .Select(ObjectMapper.Member.ToDto)
            .ToResultSetAsync(query.Options, cancellationToken);
    }

    public async Task<ResultSet<MemberDto>> GetByActiveMembershipAsync(GetMembersByActiveMembershipQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view
        EnsureIsAdminOrTrainer();

        var now = _clock.UtcNow;

        return await _context.Members
            .Where(m => m.Memberships.Any(ms =>
                ms.Status == MembershipStatus.Active &&
                ms.StartDate <= now &&
                ms.EndDate >= now))
            .Select(ObjectMapper.Member.ToDto)
            .ToResultSetAsync(query.Options, cancellationToken);
    }

    #endregion

    #region Authorization Helpers

    // Roles come from the token, so these are pure in-memory checks.
    private string RequireAuthenticatedUserId() =>
        _userContext.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

    private void EnsureIsAdmin()
    {
        RequireAuthenticatedUserId();

        if (!_userContext.IsInRole(RoleConstants.Admin))
            throw new ForbiddenException("Only administrators can perform this operation.");
    }

    private void EnsureIsAdminOrTrainer()
    {
        RequireAuthenticatedUserId();

        if (!_userContext.IsInRole(RoleConstants.Admin) &&
            !_userContext.IsInRole(RoleConstants.Trainer) &&
            !_userContext.IsInRole(RoleConstants.TrainerAdmin))
            throw new ForbiddenException("Only administrators or trainers can perform this operation.");
    }

    // Takes the already-loaded member so we don't hit the database a second time.
    private void EnsureCanModify(Member member)
    {
        var currentUserId = RequireAuthenticatedUserId();

        if (_userContext.IsInRole(RoleConstants.Admin))
            return;

        if (member.UserId != currentUserId)
            throw new ForbiddenException("You are not authorized to modify this member profile.");
    }


    // Also takes the loaded member. Only the trainer branch still needs a query,
    // because "is this member booked into one of my sessions" is contextual.
    private async Task<bool> CanSeeMedicalNotesAsync(Member member, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            return false;

        if (_userContext.IsInRole(RoleConstants.Admin))
            return true;

        // The member's own notes.
        if (member.UserId == currentUserId)
            return true;

        if (_userContext.IsInRole(RoleConstants.Trainer) ||
            _userContext.IsInRole(RoleConstants.TrainerAdmin))
        {
            return await _context.Bookings.AnyAsync(b =>
                b.MemberId == member.Id &&
                b.Status != BookingStatus.Cancelled &&
                b.ClassSession.Trainer.UserId == currentUserId,
                cancellationToken);
        }

        return false;
    }

    #endregion

    #region Helpers

    private void EnsureMinimumAge(DateOnly birthDate)
    {
        if (CalculateAge(birthDate) < _minimumAge)
            throw new BusinessRuleException($"Member must be at least {_minimumAge} years old.");
    }

    // excludeMemberId is passed on update so a member doesn't collide with itself.
    private async Task EnsureEmailIsFreeAsync(
        string email, Guid? excludeMemberId, CancellationToken cancellationToken)
    {
        var taken = await _context.Members
            .AnyAsync(m => m.Email == email && (excludeMemberId == null || m.Id != excludeMemberId), cancellationToken);

        if (taken)
            throw new BusinessRuleException($"A member with email '{email}' already exists.");
    }

    private int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow.DateTime);
        var age = today.Year - birthDate.Year;

        if (birthDate > today.AddYears(-age))
            age--;

        return age;
    }

    #endregion
}
