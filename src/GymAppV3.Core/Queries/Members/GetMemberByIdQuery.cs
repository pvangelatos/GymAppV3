namespace GymAppV3.Core.Queries.Members;

/// <summary>
/// Query to get a member by ID.
/// Returns MemberDto or MemberDetailDto depending on authorization.
/// </summary>
public record GetMemberByIdQuery(Guid MemberId);
