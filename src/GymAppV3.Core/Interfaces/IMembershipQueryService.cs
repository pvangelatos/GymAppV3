using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Memberships;

namespace GymAppV3.Core.Interfaces;

public interface IMembershipQueryService
{
    Task<MembershipDto?> GetByIdAsync(GetMembershipByIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MembershipDto>> GetByMemberAsync(GetMembershipsByMemberQuery query, CancellationToken cancellationToken = default);
}
