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
        // Validate age
        var age = CalculateAge(command.BirthDate);
        if (age < _minimumAge)
        {
            throw new BusinessRuleException($"Member must be at least {_minimumAge} years old.");
        }

        // Check if email already exists
        var existingMember = await _context.Members
            .AnyAsync(m => m.Email == command.Email, cancellationToken);

        if (existingMember)
        {
            throw new BusinessRuleException($"A member with email '{command.Email}' already exists.");
        }

        var member = new Member
        {
            UserId = command.UserId,
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
        await EnsureCanModifyMemberAsync(member.Id, cancellationToken);

        // Validate age
        var age = CalculateAge(command.BirthDate);
        if (age < _minimumAge)
        {
            throw new BusinessRuleException($"Member must be at least {_minimumAge} years old.");
        }

        // Check if email change conflicts with another member
        if (member.Email != command.Email)
        {
            var emailExists = await _context.Members
                .AnyAsync(m => m.Email == command.Email && m.Id != member.Id, cancellationToken);

            if (emailExists)
            {
                throw new BusinessRuleException($"A member with email '{command.Email}' already exists.");
            }

            // Sync email to IdentityUser if member has a user account
            if (member.UserId != null)
            {
                var user = await _userManager.FindByIdAsync(member.UserId);
                if (user != null)
                {
                    user.Email = command.Email;
                    user.UserName = command.Email;
                    user.NormalizedEmail = command.Email.ToUpperInvariant();
                    user.NormalizedUserName = command.Email.ToUpperInvariant();
                    await _userManager.UpdateAsync(user);
                }
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

        return member.ToDto();
    }

    public async Task DeleteAsync(DeleteMemberCommand command, CancellationToken cancellationToken = default)
    {
        var member = await _context.Members
            .Include(m => m.Memberships)
            .Include(m => m.Bookings)
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(Member), command.MemberId);

        // Authorization check: Only Admin can delete members
        await EnsureIsAdminAsync();

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

        // Check if current user can see medical notes
        var canSeeMedicalNotes = await CanSeeMedicalNotesAsync(member.Id, cancellationToken);

        // Always return DetailDto (with or without medical notes based on authorization)
        if (!canSeeMedicalNotes)
        {
            // Return DetailDto but with null medical notes
            return new MemberDetailDto(
                Id: member.Id,
                UserId: member.UserId,
                Firstname: member.Firstname,
                Lastname: member.Lastname,
                Email: member.Email,
                Phone: member.Phone,
                Address: member.Address.ToDto(),
                BirthDate: member.BirthDate,
                HasMedicalConditions: member.HasMedicalConditions,
                MedicalNotes: null);
        }

        return member.ToDetailDto();
    }

    public async Task<ResultSet<MemberDto>> GetAllAsync(GetAllMembersQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view all members
        await EnsureIsAdminOrTrainerAsync();

        var membersQuery = _context.Members.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            membersQuery = membersQuery.Where(m =>
                m.Firstname.ToLower().Contains(search) ||
                m.Lastname.ToLower().Contains(search) ||
                m.Email.ToLower().Contains(search));
        }

        // Apply active membership filter
        if (query.HasActiveMembership.HasValue)
        {
            if (query.HasActiveMembership.Value)
            {
                var now = _clock.UtcNow;
                membersQuery = membersQuery.Where(m =>
                    m.Memberships.Any(ms =>
                        ms.Status == MembershipStatus.Active &&
                        ms.StartDate <= now &&
                        ms.EndDate >= now));
            }
            else
            {
                var now = _clock.UtcNow;
                membersQuery = membersQuery.Where(m =>
                    !m.Memberships.Any(ms =>
                        ms.Status == MembershipStatus.Active &&
                        ms.StartDate <= now &&
                        ms.EndDate >= now));
            }
        }

        // Apply pagination
        var resultSet = await membersQuery.ToResultSetAsync(query.Options, cancellationToken);

        // Map to DTOs
        var dtos = resultSet.Items.Select(m => m.ToDto()).ToList();

        return new ResultSet<MemberDto>(dtos, resultSet.Count, query.Options?.Size ?? 50);
    }

    public async Task<ResultSet<MemberDto>> GetByActiveBookingsAsync(GetMembersByActiveBookingsQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view
        await EnsureIsAdminOrTrainerAsync();

        var now = _clock.UtcNow;

        var membersQuery = _context.Members
            .Where(m => m.Bookings.Any(b =>
                b.Status != BookingStatus.Cancelled &&
                b.ClassSession.StartsAt > now))
            .Distinct();

        var resultSet = await membersQuery.ToResultSetAsync(query.Options, cancellationToken);

        var dtos = resultSet.Items.Select(m => m.ToDto()).ToList();

        return new ResultSet<MemberDto>(dtos, resultSet.Count, query.Options?.Size ?? 50);
    }

    public async Task<ResultSet<MemberDto>> GetByActiveMembershipAsync(GetMembersByActiveMembershipQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Only Admin/Trainer can view
        await EnsureIsAdminOrTrainerAsync();

        var now = _clock.UtcNow;

        var membersQuery = _context.Members
            .Where(m => m.Memberships.Any(ms =>
                ms.Status == MembershipStatus.Active &&
                ms.StartDate <= now &&
                ms.EndDate >= now));

        var resultSet = await membersQuery.ToResultSetAsync(query.Options, cancellationToken);

        var dtos = resultSet.Items.Select(m => m.ToDto()).ToList();

        return new ResultSet<MemberDto>(dtos, resultSet.Count, query.Options?.Size ?? 50);
    }

    #endregion

    #region Authorization Helpers

    private async Task<bool> CanSeeMedicalNotesAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            return false;

        // Get current user's roles
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            return false;

        var roles = await _userManager.GetRolesAsync(user);

        // Admin can see all medical notes
        if (roles.Contains(RoleConstants.Admin))
            return true;

        // Member can see their own medical notes
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member != null && member.UserId == currentUserId)
            return true;

        // Trainer can see medical notes for members with active bookings in their sessions
        if (roles.Contains(RoleConstants.Trainer))
        {
            var hasBookingWithTrainer = await _context.Bookings
                .AnyAsync(b =>
                    b.MemberId == memberId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.ClassSession.Trainer.UserId == currentUserId,
                    cancellationToken);

            return hasBookingWithTrainer;
        }

        return false;
    }

    private async Task EnsureCanModifyMemberAsync(Guid memberId, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        // Admin can modify any member
        if (roles.Contains(RoleConstants.Admin))
            return;

        // Member can only modify their own profile
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

        if (member == null || member.UserId != currentUserId)
            throw new UnauthorizedAccessException("You are not authorized to modify this member profile.");
    }

    private async Task EnsureIsAdminAsync()
    {
        var currentUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains(RoleConstants.Admin))
            throw new UnauthorizedAccessException("Only administrators can perform this operation.");
    }

    private async Task EnsureIsAdminOrTrainerAsync()
    {
        var currentUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains(RoleConstants.Admin) && !roles.Contains(RoleConstants.Trainer))
            throw new UnauthorizedAccessException("Only administrators or trainers can perform this operation.");
    }

    #endregion

    #region Helpers

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
