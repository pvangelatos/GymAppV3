using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IMembershipPackageCommandService
{
    Task<MembershipPackageDto> CreateAsync(CreateMembershipPackageCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateMembershipPackageCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
