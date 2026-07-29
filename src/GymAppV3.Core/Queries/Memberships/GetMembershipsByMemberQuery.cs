using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Memberships;

public record GetMembershipsByMemberQuery(
    Guid MemberId,
    bool OnlyActive = false) : IQuery<IReadOnlyList<MembershipDto>>;
