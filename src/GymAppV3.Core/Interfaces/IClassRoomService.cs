using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    public interface IClassRoomService
    {
        // Returns all active (non-soft-deleted) rooms.
        Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(CancellationToken cancellationToken = default);

        // Returns a single room, or null if it does not exist.
        Task<ClassRoomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Creates a new room and returns the created record.
        Task<ClassRoomDto> CreateAsync(CreateClassRoomCommand request, CancellationToken cancellationToken = default);

        // Updates an existing room. Throws NotFoundException if it does not exist.
        Task UpdateAsync(Guid id, UpdateClassRoomCommand request, CancellationToken cancellationToken = default);

        // Soft-deletes a room (the interceptor converts the delete to an update).
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
