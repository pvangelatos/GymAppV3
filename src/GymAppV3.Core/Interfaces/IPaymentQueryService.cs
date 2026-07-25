using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Payments;

namespace GymAppV3.Core.Interfaces;

public interface IPaymentQueryService
{
    Task<ResultSet<PaymentDto>> GetPaymentsByMemberIdAsync(GetPaymentsByMemberQuery query,
    CancellationToken cancellationToken = default);

    Task<MonthlyFinancialReportDto> GetMonthlyFinancialReportAsync(GetMonthlyFinancialReportQuery query,
    CancellationToken cancellationToken = default);

}
