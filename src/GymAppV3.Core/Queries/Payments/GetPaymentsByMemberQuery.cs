using GymAppV3.Core.DTOs;
using GymAppV3.Core.Abstractions;

namespace GymAppV3.Core.Queries.Payments;

public record GetPaymentsByMemberQuery(Guid MemberId) : IQuery<IReadOnlyList<PaymentDto>>;

