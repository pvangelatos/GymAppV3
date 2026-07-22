using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.Payment;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        group.MapPost("/", PaymentHandlers.RecordAsync)
            .WithName("RecordPayment")
            .RequireAuthorization()
            .Accepts<RecordPaymentCommand>("application/json")
            .Produces<PaymentDto>(StatusCodes.Status201Created);

        group.MapGet("/member/{memberId:guid}", PaymentHandlers.GetByMemberAsync)
            .WithName("GetPaymentsByMember")
            .RequireAuthorization()
            .Produces<IReadOnlyList<PaymentDto>>(StatusCodes.Status200OK);

        group.MapGet("/reports/monthly", PaymentHandlers.GetMonthlyReportAsync)
            .WithName("GetMonthlyFinancialReport")
            .RequireAuthorization("AdminOnly")
            .Produces<MonthlyFinancialReportDto>(StatusCodes.Status200OK);

        return app;
    }
}
