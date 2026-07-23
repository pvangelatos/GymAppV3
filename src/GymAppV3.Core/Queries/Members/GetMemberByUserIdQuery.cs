using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Members;

public record GetMemberByUserIdQuery(string UserId) : IQuery<MemberDto>;
