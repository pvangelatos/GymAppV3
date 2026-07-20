using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Payments;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GymAppV3.Core.Abstractions;


namespace GymAppV3.Infrastructure.Handlers.Queries.Payments
{
    public class GetPaymentsByMemberQueryHandler : IQueryHandler<GetPaymentsByMemberQuery, IReadOnlyList<PaymentDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetPaymentsByMemberQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<PaymentDto>> HandleAsync(GetPaymentsByMemberQuery query,
        CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Where(p => p.MemberId == query.MemberId)
                .OrderByDescending(p => p.PaidAt)
                .Select(ObjectMapper.Payment.ToDto)
                .ToListAsync(cancellationToken);
        }
    }
}
