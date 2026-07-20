using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.MembershipPackages;

public record GetMembershipPackageByIdQuery(Guid Id) : IQuery<MembershipPackageDto?>;
