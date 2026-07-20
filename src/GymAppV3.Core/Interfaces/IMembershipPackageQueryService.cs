using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.MembershipPackages;

namespace GymAppV3.Core.Interfaces;

public interface IMembershipPackageQueryService
{
    Task<IReadOnlyList<MembershipPackageDto>> GetAllAsync(GetAllMembershipPackagesQuery query, CancellationToken cancellationToken = default);
    Task<MembershipPackageDto?> GetByIdAsync(GetMembershipPackageByIdQuery query, CancellationToken cancellationToken = default);
}
