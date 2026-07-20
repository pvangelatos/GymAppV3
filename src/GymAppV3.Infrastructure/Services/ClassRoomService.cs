
using System.Linq.Expressions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.ClassRooms;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

public class ClassRoomService : IClassRoomCommandService, IClassRoomQueryService
{
    private readonly ApplicationDbContext _context;

    public ClassRoomService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(
        GetAllClassRoomsQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.ClassRooms
            .Select(ObjectMapper.ClassRoom.ToDto)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassRoomDto?> GetByIdAsync(
        GetClassRoomByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.ClassRooms
            .Where(r => r.Id == query.Id)
            .Select(ObjectMapper.ClassRoom.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClassRoomDto> CreateAsync(
        CreateClassRoomCommand request, CancellationToken cancellationToken = default)
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

        return ObjectMapper.ClassRoom.ToDtoCompiled(room);
    }

    public async Task UpdateAsync(
        Guid id, UpdateClassRoomCommand request, CancellationToken cancellationToken = default)
    {
        var room = await _context.ClassRooms
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), id);

        room.ClassRoomName = request.ClassRoomName;
        room.Capacity = request.Capacity;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _context.ClassRooms
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}