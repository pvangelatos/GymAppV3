using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

/// <summary>
/// Service interface for member command operations (Create, Update, Delete).
/// </summary>
public interface IMemberCommandService
{
    /// <summary>
    /// Creates a new member profile.
    /// Can be used by Admin/Trainer to create members without user accounts.
    /// </summary>
    Task<MemberDto> CreateAsync(CreateMemberCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing member profile.
    /// Also syncs email changes to IdentityUser if UserId is present.
    /// </summary>
    Task<MemberDto> UpdateAsync(UpdateMemberCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a member and cascades to related entities.
    /// </summary>
    Task DeleteAsync(DeleteMemberCommand command, CancellationToken cancellationToken = default);
}
