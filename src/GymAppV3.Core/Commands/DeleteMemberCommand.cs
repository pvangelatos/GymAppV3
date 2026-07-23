namespace GymAppV3.Core.Commands;

/// <summary>
/// Command to soft-delete a member.
/// Cascades soft-delete to related Memberships and cancels active Bookings.
/// Payments are preserved for financial history.
/// </summary>
public record DeleteMemberCommand(Guid MemberId);
