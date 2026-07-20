using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IPaymentCommandService
{
    Task<PaymentDto> RecordAsync(RecordPaymentCommand command, CancellationToken cancellationToken = default);
}
