using GymAppv3.Server.Endpoints.Common;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Common;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Payments;

namespace GymAppv3.Server.Endpoints.Payment;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var payments = app.MapGroup("/api/payments")
            .WithTags("Payments");

        var memberPayments = app.MapGroup("/api/members/{memberId:guid}/payments")
            .WithTags("Payments")
            .AddEndpointFilter<MemberScopedFilter>();

        payments.MapPost("/", PaymentHandlers.RecordAsync)
            .WithName("RecordPayment")
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<RecordPaymentCommand>>()
            .Accepts<RecordPaymentCommand>("application/json")
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<PaymentDto>(StatusCodes.Status201Created);

        memberPayments.MapGet("/", PaymentHandlers.GetByMemberAsync)
            .WithName("GetPaymentsByMember")
            .RequireAuthorization()
            .Produces<ResultSet<PaymentDto>>(StatusCodes.Status200OK);

        payments.MapGet("/reports/monthly", PaymentHandlers.GetMonthlyReportAsync)
            .WithName("GetMonthlyFinancialReport")
            .RequireAuthorization("AdminOnly")
            .AddEndpointFilter<ValidationFilter<GetMonthlyFinancialReportQuery>>()
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .Produces<MonthlyFinancialReportDto>(StatusCodes.Status200OK);

        return app;
    }
}
