using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.Payments;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.Payment;

public static class PaymentHandlers
{
    public static async Task<Created<PaymentDto>> RecordAsync(
        RecordPaymentCommand command,
        IPaymentCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.RecordAsync(command, cancellationToken);
        return TypedResults.Created($"/api/payments/{created.Id}", created);
    }

    public static async Task<Ok<IReadOnlyList<PaymentDto>>> GetByMemberAsync(
        Guid memberId,
        IPaymentQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetPaymentsByMemberIdAsync(new GetPaymentsByMemberQuery(memberId), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Ok<MonthlyFinancialReportDto>> GetMonthlyReportAsync(
        int year,
        int month,
        IPaymentQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetMonthlyFinancialReportAsync(new GetMonthlyFinancialReportQuery(year, month), cancellationToken);
        return TypedResults.Ok(result);
    }
}
