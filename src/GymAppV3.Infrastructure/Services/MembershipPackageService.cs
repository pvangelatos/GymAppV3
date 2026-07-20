
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Infrastructure.Services;

// CRUD implementation using IApplicationDbContext directly — no repository layer.
// The soft-delete global query filter means "deleted" rows are invisible here
// automatically; none of these methods need to check IsDeleted by hand.
public class MembershipPackageService : IMembershipPackageService
{
    private readonly ApplicationDbContext _context;

    public MembershipPackageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MembershipPackageDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // Project straight to the DTO in the query, so EF selects only the needed
        // columns and never materialises the full entity. AsNoTracking is implied
        // by projecting to a non-entity type.
        return await _context.MembershipPackages
            .Select(ObjectMapper.MembershipPackage.ToDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<MembershipPackageDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        // FirstOrDefaultAsync (not FindAsync) — FindAsync bypasses global query
        // filters, so it would return soft-deleted rows. This respects the filter.
        return await _context.MembershipPackages
            .Where(p => p.Id == id)
            .Select(ObjectMapper.MembershipPackage.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MembershipPackageDto> CreateAsync(
        CreateMembershipPackageCommand request, CancellationToken cancellationToken = default)
    {
        // The referenced category must exist (and be active).
        var category = await _context.ClassCategories
            .FirstOrDefaultAsync(c => c.Id == request.ClassCategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);

        var package = new MembershipPackage
        {
            Name = request.Name,
            Price = request.Price,
            DurationInDays = request.DurationInDays,
            SessionsIncluded = request.SessionsIncluded,
            ClassCategoryId = request.ClassCategoryId
        };

        _context.MembershipPackages.Add(package);
        await _context.SaveChangesAsync(cancellationToken);

        return ObjectMapper.MembershipPackage.ToDtoCompiled(package);
    }

    public async Task UpdateAsync(
        Guid id, UpdateMembershipPackageCommand request, CancellationToken cancellationToken = default)
    {
        // Here we DO need the tracked entity (not a projection), because we mutate
        // it and let the change tracker generate the UPDATE.
        var package = await _context.MembershipPackages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MembershipPackage), id);

        if (package.ClassCategoryId != request.ClassCategoryId)
        {
            var categoryExists = await _context.ClassCategories
                .AnyAsync(c => c.Id == request.ClassCategoryId, cancellationToken);
            if (!categoryExists)
                throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);
        }

        package.Name = request.Name;
        package.Price = request.Price;
        package.DurationInDays = request.DurationInDays;
        package.SessionsIncluded = request.SessionsIncluded;
        package.ClassCategoryId = request.ClassCategoryId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _context.MembershipPackages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(MembershipPackage), id);

        // Remove() marks the entity Deleted; the AuditableEntityInterceptor converts
        // that into a soft delete (UPDATE IsDeleted = 1) before it reaches the database.
        _context.MembershipPackages.Remove(package);
        await _context.SaveChangesAsync(cancellationToken);
    }
}