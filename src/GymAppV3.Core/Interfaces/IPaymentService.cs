using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces
{
    // Records payments and produces monthly financial summaries. Amounts are stored
    // gross (VAT included); the service derives net and VAT figures using each
    // payment's own VatRate snapshot.
    public interface IPaymentService
    {
        // Records a payment. Validates that the member (and membership, if given) exist.
        Task<PaymentDto> RecordAsync(
            RecordPaymentCommand request, CancellationToken cancellationToken = default);

        // Returns a member's payment history, newest first.
        Task<IReadOnlyList<PaymentDto>> GetPaymentsByMemberAsync(
            Guid memberId, CancellationToken cancellationToken = default);

        // Produces a financial summary (gross/net/VAT totals) for a given month.
        Task<MonthlyFinancialReportDto> GetMonthlyReportAsync(
            int year, int month, CancellationToken cancellationToken = default);
    }        
}
