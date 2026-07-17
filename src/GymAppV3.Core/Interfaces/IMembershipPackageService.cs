using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    public interface IMembershipPackageService
    {
        // Returns all active(non - soft - deleted) packages.
        Task<IReadOnlyList<MembershipPackageDto>> GetAllAsync(CancellationToken cancellationToken = default);

        // Returns a single package, or null if it does not exist.
        Task<MembershipPackageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Creates a new package and returns the created record.
        Task<MembershipPackageDto> CreateAsync(CreateMembershipPackageCommand request, CancellationToken cancellationToken = default);

        // Updates an existing package. Throws NotFoundException if it does not exist.
        Task UpdateAsync(Guid id, UpdateMembershipPackageCommand request, CancellationToken cancellationToken = default);

        // Soft-deletes a package (the interceptor converts the delete to an update).
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
