using GymAppV3.Core.DTOs;
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Common;

namespace GymAppV3.Core.Queries.Payments;

public record GetPaymentsByMemberQuery(Guid MemberId, ListOptions? Options = null) : IQuery<ResultSet<PaymentDto>>;

