using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Memberships;

public record GetMembershipByIdQuery(Guid Id) : IQuery<MembershipDto?>;
