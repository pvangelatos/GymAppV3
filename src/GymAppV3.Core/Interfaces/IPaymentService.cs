using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces
{
    // Records payments 
    public interface IPaymentService
    {
        // Records a payment. Validates that the member (and membership, if given) exist.
        Task<PaymentDto> RecordAsync(
            RecordPaymentCommand request, CancellationToken cancellationToken = default);
    }        
}
