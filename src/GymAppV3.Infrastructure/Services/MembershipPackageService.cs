using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.MembershipPackages;

namespace GymAppV3.Infrastructure.Services;

// CRUD implementation using IApplicationDbContext directly — no repository layer.
// The soft-delete global query filter means "deleted" rows are invisible here
// automatically; none of these methods need to check IsDeleted by hand.
public class MembershipPackageService : IMembershipPackageCommandService, IMembershipPackageQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IClassCategoryCapacityService _capacityService;

    public MembershipPackageService(ApplicationDbContext context, IClassCategoryCapacityService capacityService)
    {
        _context = context;
        _capacityService = capacityService;
    }

    public async Task<IReadOnlyList<MembershipPackageDto>> GetAllAsync(
        GetAllMembershipPackagesQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.MembershipPackages
            .Select(ObjectMapper.MembershipPackage.ToDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<MembershipPackageDto?> GetByIdAsync(
        GetMembershipPackageByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.MembershipPackages
            .Where(p => p.Id == query.Id)
            .Select(ObjectMapper.MembershipPackage.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MembershipPackageDto> CreateAsync(
        CreateMembershipPackageCommand request, CancellationToken cancellationToken = default)
    {
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

        _context.MembershipPackages.Remove(package);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MembershipPackageAvailabilityDto>> GetAvailabilityAsync(
        CancellationToken cancellationToken = default)
    {
        var packages = await _context.MembershipPackages
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.SessionsIncluded,
                p.DurationInDays,
                p.ClassCategoryId,
                CategoryName = p.ClassCategory.Name
            })
            .ToListAsync(cancellationToken);

        var result = new List<MembershipPackageAvailabilityDto>(packages.Count);

        // Cache supply/demand per category so packages sharing a category don't
        // re-run the same two queries.
        var categoryCapacity = new Dictionary<Guid, (double Supply, double Demand)>();

        foreach (var package in packages)
        {
            if (!categoryCapacity.TryGetValue(package.ClassCategoryId, out var capacity))
            {
                capacity = await _capacityService.GetWeeklyCapacityAsync(package.ClassCategoryId, cancellationToken);
                categoryCapacity[package.ClassCategoryId] = capacity;
            }

            var rate = _capacityService.GetWeeklyRate(package.SessionsIncluded, package.DurationInDays);
            var remaining = capacity.Supply - capacity.Demand;
            var availableSlots = rate > 0 ? Math.Max(0, (int)Math.Floor(remaining / rate)) : 0;

            result.Add(new MembershipPackageAvailabilityDto(
                package.Id, package.Name, package.ClassCategoryId, package.CategoryName,
                capacity.Supply, capacity.Demand, rate, availableSlots));
        }

        return result;
    }
}