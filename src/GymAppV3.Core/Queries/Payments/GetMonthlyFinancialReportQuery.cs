using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Payments;

public record GetMonthlyFinancialReportQuery(int Year, int Month) : IQuery<MonthlyFinancialReportDto>;
