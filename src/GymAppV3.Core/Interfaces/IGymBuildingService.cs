using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    public interface IGymBuildingService
    {
        // Returns all active (non-soft-deleted) buildings.
        Task<IReadOnlyList<GymBuildingDto>> GetAllAsync(CancellationToken cancellationToken = default);

        // Returns a single building, or null if it does not exist.
        Task<GymBuildingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Creates a new building and returns the created record.
        Task<GymBuildingDto> CreateAsync(CreateGymBuildingCommand request, CancellationToken cancellationToken = default);

        // Updates an existing building. Throws NotFoundException if it does not exist.
        Task UpdateAsync(Guid id, UpdateGymBuildingCommand request, CancellationToken cancellationToken = default);

        // Soft-deletes a building (the interceptor converts the delete to an update).
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
