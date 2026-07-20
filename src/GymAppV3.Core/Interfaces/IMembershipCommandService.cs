using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IMembershipCommandService
{
    Task<MembershipDto> PurchaseAsync(PurchaseMembershipCommand command, CancellationToken cancellationToken = default);
}
