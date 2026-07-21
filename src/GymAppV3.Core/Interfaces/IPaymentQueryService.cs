using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Payments;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces;

public interface IPaymentQueryService
{
    Task<IReadOnlyList<PaymentDto>> GetPaymentsByMemberIdAsync(GetPaymentsByMemberQuery query,
    CancellationToken cancellationToken = default);

    Task<MonthlyFinancialReportDto> GetMonthlyFinancialReportAsync(GetMonthlyFinancialReportQuery query,
    CancellationToken cancellationToken = default);

}
