
using GymAppV3.Application.Exceptions;
using GymAppV3.Core.DTOs.ClassRoom;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly ApplicationDbContext _context;

    public ClassRoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ClassRooms
            .Select(r => new ClassRoomDto(r.Id, r.ClassRoomName, r.Capacity, r.GymBuildingId))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassRoomDto?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ClassRooms
            .Where(r => r.Id == id)
            .Select(r => new ClassRoomDto(r.Id, r.ClassRoomName, r.Capacity, r.GymBuildingId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClassRoomDto> CreateAsync(
        CreateClassRoomRequest request, CancellationToken cancellationToken = default)
    {
        // Referential validation: the target building must exist (and be active).
        // AnyAsync respects the soft-delete query filter, so a deleted building fails.
        var buildingExists = await _context.GymBuildings
            .AnyAsync(b => b.Id == request.GymBuildingId, cancellationToken);

        if (!buildingExists)
            throw new NotFoundException(nameof(GymBuilding), request.GymBuildingId);

        var room = new ClassRoom
        {
            ClassRoomName = request.ClassRoomName,
            Capacity = request.Capacity,
            GymBuildingId = request.GymBuildingId
        };

        _context.ClassRooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClassRoomDto(room.Id, room.ClassRoomName, room.Capacity, room.GymBuildingId);
    }

    public async Task UpdateAsync(
        Guid id, UpdateClassRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await _context.ClassRooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), id);

        // If the building is being changed, validate the new one exists.
        if (room.GymBuildingId != request.GymBuildingId)
        {
            var buildingExists = await _context.GymBuildings
                .AnyAsync(b => b.Id == request.GymBuildingId, cancellationToken);

            if (!buildingExists)
                throw new NotFoundException(nameof(GymBuilding), request.GymBuildingId);
        }

        room.ClassRoomName = request.ClassRoomName;
        room.Capacity = request.Capacity;
        room.GymBuildingId = request.GymBuildingId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _context.ClassRooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), id);

        _context.ClassRooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
    }
}