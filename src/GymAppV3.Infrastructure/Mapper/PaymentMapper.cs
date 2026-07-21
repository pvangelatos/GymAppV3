using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers;

internal static class PaymentMapper
{
    // VatAmount isn't stored on the entity — it's derived (Amount - NetAmount),
    // same as the comment on Payment.NetAmount says. Deriving it here keeps that
    // rule in one place instead of duplicating it in every service that reads payments.
    public static readonly Expression<Func<Payment, PaymentDto>> ToDto =
        p => new PaymentDto(
            p.Id,
            p.MemberId,
            p.MembershipId,
            p.Amount,
            p.NetAmount,
            p.Amount - p.NetAmount,
            p.VatRate,
            p.Method.ToString(),
            p.Status.ToString(),
            p.PaidAt);

    public static readonly Func<Payment, PaymentDto> ToDtoCompiled = ToDto.Compile();
}
