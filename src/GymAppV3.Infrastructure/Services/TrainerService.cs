using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.Trainers;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using GymAppV3.Infrastructure.Mapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

/// <summary>
/// Manages trainer profiles. Creating a trainer also provisions the login account,
/// so this service owns both the domain entity and its Identity user.
/// </summary>
public class TrainerService : ITrainerCommandService, ITrainerQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _clock;
    private readonly UserManager<IdentityUser> _userManager;

    public TrainerService(
        ApplicationDbContext context,
        IUserContext userContext,
        IDateTimeProvider clock,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userContext = userContext;
        _clock = clock;
        _userManager = userManager;
    }

    #region Command Operations

    public async Task<TrainerCreatedDto> CreateAsync(
        CreateTrainerCommand command, CancellationToken cancellationToken = default)
    {
        EnsureIsAdmin();

        await EnsureEmailIsFreeAsync(command.Email, null, cancellationToken);
        await EnsureCategoriesExistAsync(command.SpecialtyCategoryIds, cancellationToken);

        var password = TemporaryPasswordGenerator.Generate();

        // UserManager writes through the same scoped DbContext, so a single
        // transaction covers both the account and the profile.
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var user = new IdentityUser
        {
            UserName = command.Email,
            Email = command.Email,
            // Admin-created and handed over in person — there is no confirmation mail to send.
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new BusinessRuleException(Describe(createResult));

        var roleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Trainer);
        if (!roleResult.Succeeded)
            throw new BusinessRuleException(Describe(roleResult));

        var trainer = new Trainer
        {
            UserId = user.Id,
            Firstname = command.Firstname,
            Lastname = command.Lastname,
            Email = command.Email,
            Phone = command.Phone,
            Bio = command.Bio,
            // TrainerId is filled in by relationship fixup when the trainer is added.
            Specialties = command.SpecialtyCategoryIds
                .Distinct()
                .Select(categoryId => new TrainerSpecialty { ClassCategoryId = categoryId })
                .ToList()
        };

        _context.Trainers.Add(trainer);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Re-read through the projection so the category names come back populated.
        var dto = await _context.Trainers
            .Where(t => t.Id == trainer.Id)
            .Select(ObjectMapper.Trainer.ToDto)
            .FirstAsync(cancellationToken);

        // The only moment the password is ever readable.
        return new TrainerCreatedDto(dto, password);
    }

    public async Task UpdateAsync(
        Guid id, UpdateTrainerCommand command, CancellationToken cancellationToken = default)
    {
        var trainer = await _context.Trainers
            .Include(t => t.Specialties)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Trainer), id);

        EnsureCanModify(trainer);

        await EnsureEmailIsFreeAsync(command.Email, trainer.Id, cancellationToken);
        await EnsureCategoriesExistAsync(command.SpecialtyCategoryIds, cancellationToken);

        var emailChanged = trainer.Email != command.Email;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (emailChanged)
        {
            var user = await _userManager.FindByIdAsync(trainer.UserId);
            if (user is not null)
            {
                // These go through the configured ILookupNormalizer — don't set
                // the Normalized* fields by hand.
                var emailResult = await _userManager.SetEmailAsync(user, command.Email);
                if (!emailResult.Succeeded)
                    throw new BusinessRuleException(Describe(emailResult));

                var nameResult = await _userManager.SetUserNameAsync(user, command.Email);
                if (!nameResult.Succeeded)
                    throw new BusinessRuleException(Describe(nameResult));
            }
        }

        trainer.Firstname = command.Firstname;
        trainer.Lastname = command.Lastname;
        trainer.Email = command.Email;
        trainer.Phone = command.Phone;
        trainer.Bio = command.Bio;

        SyncSpecialties(trainer, command.SpecialtyCategoryIds);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureIsAdmin();

        var trainer = await _context.Trainers
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Trainer), id);

        trainer.IsDeleted = true;
        trainer.DeletedAt = _clock.UtcNow;
        trainer.DeletedBy = _userContext.UserId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Query Operations

    public async Task<IReadOnlyList<TrainerDto>> GetAllAsync(GetAllTrainersQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Trainers
             .OrderBy(t => t.Lastname)
             .Select(ObjectMapper.Trainer.ToDto)
             .ToListAsync(cancellationToken);
    }

    public async Task<TrainerDto?> GetByIdAsync(GetTrainerByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Trainers
             .Where(t => t.Id == query.Id)
             .Select(ObjectMapper.Trainer.ToDto)
             .FirstOrDefaultAsync(cancellationToken); 
    }

    public async Task<TrainerDto?> GetByUserIdAsync(GetTrainerByUserIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.Trainers
            .Where(t => t.UserId == query.UserId)
            .Select(ObjectMapper.Trainer.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Brings the trainer's specialty rows in line with the requested category list:
    /// removes what is no longer wanted, adds what is missing, leaves the rest alone.
    /// </summary>
    private static void SyncSpecialties(Trainer trainer, IReadOnlyList<Guid> requestedIds)
    {
        var requested = requestedIds.ToHashSet();
        var current = trainer.Specialties.Select(s => s.ClassCategoryId).ToHashSet();

        // ToList() because we are mutating the collection we iterate.
        foreach (var stale in trainer.Specialties.Where(s => !requested.Contains(s.ClassCategoryId)).ToList())
            trainer.Specialties.Remove(stale);

        foreach (var categoryId in requested.Except(current))
            trainer.Specialties.Add(new TrainerSpecialty
            {
                TrainerId = trainer.Id,
                ClassCategoryId = categoryId
            });
    }

    // Without this a bad category id surfaces as an FK violation — a 500 instead of a 422.
    private async Task EnsureCategoriesExistAsync(
        IReadOnlyList<Guid> categoryIds, CancellationToken cancellationToken)
    {
        var distinct = categoryIds.Distinct().ToList();
        if (distinct.Count == 0)
            return;

        var found = await _context.ClassCategories
            .CountAsync(c => distinct.Contains(c.Id), cancellationToken);

        if (found != distinct.Count)
            throw new BusinessRuleException("One or more specialty categories do not exist.");
    }

    private async Task EnsureEmailIsFreeAsync(
        string email, Guid? excludeTrainerId, CancellationToken cancellationToken)
    {
        var taken = await _context.Trainers
            .AnyAsync(t => t.Email == email && (excludeTrainerId == null || t.Id != excludeTrainerId),
                cancellationToken);

        if (taken)
            throw new BusinessRuleException($"A trainer with email '{email}' already exists.");
    }

    private static string Describe(IdentityResult result)
        => string.Join("; ", result.Errors.Select(e => e.Description));

    #endregion

    #region Authorization Helpers

    private string RequireAuthenticatedUserId()
        => _userContext.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

    private void EnsureIsAdmin()
    {
        RequireAuthenticatedUserId();

        if (!_userContext.IsInRole(RoleConstants.Admin))
            throw new ForbiddenException("Only administrators can perform this operation.");
    }

    // Admins edit anyone; a trainer may edit their own profile.
    private void EnsureCanModify(Trainer trainer)
    {
        var currentUserId = RequireAuthenticatedUserId();

        if (_userContext.IsInRole(RoleConstants.Admin))
            return;

        if (trainer.UserId != currentUserId)
            throw new ForbiddenException("You are not authorized to modify this trainer profile.");
    }

    #endregion

}
