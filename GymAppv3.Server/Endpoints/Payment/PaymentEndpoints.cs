using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Payment;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var payments = app.MapGroup("/api/payments")
            .WithTags("Payments");

        var memberPayments = app.MapGroup("/api/members/{memberId:guid}/payments")
            .WithTags("Payments");

        payments.MapPost("/", PaymentHandlers.RecordAsync)
            .WithName("RecordPayment")
            .RequireAuthorization()
            .Accepts<RecordPaymentCommand>("application/json")
            .Produces<PaymentDto>(StatusCodes.Status201Created);

        memberPayments.MapGet("/", PaymentHandlers.GetByMemberAsync)
            .WithName("GetPaymentsByMember")
            .RequireAuthorization()
            .Produces<IReadOnlyList<PaymentDto>>(StatusCodes.Status200OK);

        payments.MapGet("/reports/monthly", PaymentHandlers.GetMonthlyReportAsync)
            .WithName("GetMonthlyFinancialReport")
            .RequireAuthorization("AdminOnly")
            .Produces<MonthlyFinancialReportDto>(StatusCodes.Status200OK);

        return app;
    }
}
