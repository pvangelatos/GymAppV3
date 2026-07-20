using GymAppV3.Core.Exceptions;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.GymBuildings;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GymAppV3.Core.Commands;

namespace GymAppV3.Infrastructure.Services;

public class GymBuildingService : IGymBuildingCommandService, IGymBuildingQueryService
{
    private readonly ApplicationDbContext _context;

    public GymBuildingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GymBuildingDto>> GetAllAsync(
        GetAllGymBuildingsQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.GymBuildings
            .Select(ObjectMapper.GymBuilding.ToDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<GymBuildingDto?> GetByIdAsync(
        GetGymBuildingByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.GymBuildings
            .Where(b => b.Id == query.Id)
            .Select(ObjectMapper.GymBuilding.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GymBuildingDto> CreateAsync(
        CreateGymBuildingCommand request, CancellationToken cancellationToken = default)
    {
        var building = new GymBuilding
        {
            Name = request.Name,
            Description = request.Description,
            // Map the incoming AddressDto into the owned Address value object.
            Address = new Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                ZipCode = request.Address.ZipCode,
                Country = request.Address.Country
            },
            Phone = request.Phone,
            Email = request.Email
        };

        _context.GymBuildings.Add(building);
        await _context.SaveChangesAsync(cancellationToken);

        return ObjectMapper.GymBuilding.ToDtoCompiled(building);
    }

    public async Task UpdateAsync(
        Guid id, UpdateGymBuildingCommand request, CancellationToken cancellationToken = default)
    {
        var building = await _context.GymBuildings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(GymBuilding), id);

        building.Name = request.Name;
        building.Description = request.Description;
        // Replace the whole owned Address — value objects are updated wholesale,
        // not field by field.
        building.Address = new Address
        {
            Street = request.Address.Street,
            City = request.Address.City,
            State = request.Address.State,
            ZipCode = request.Address.ZipCode,
            Country = request.Address.Country
        };
        building.Phone = request.Phone;
        building.Email = request.Email;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var building = await _context.GymBuildings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(GymBuilding), id);

        _context.GymBuildings.Remove(building);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
