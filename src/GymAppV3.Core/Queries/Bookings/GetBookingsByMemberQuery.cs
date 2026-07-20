using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.Bookings;

public record GetBookingsByMemberQuery(Guid MemberId) : IQuery<IReadOnlyList<BookingDto>>;
