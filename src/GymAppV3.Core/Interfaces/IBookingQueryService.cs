using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.Bookings;

namespace GymAppV3.Core.Interfaces;

public interface IBookingQueryService
{
    Task<IReadOnlyList<BookingDto>> GetByMemberAsync(GetBookingsByMemberQuery query, CancellationToken cancellationToken = default);
}
