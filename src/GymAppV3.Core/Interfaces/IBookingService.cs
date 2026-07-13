using GymAppV3.Core.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    // Use-case service for booking and cancelling class sessions. This is the most
    // coordinated operation in the system: a booking atomically creates the booking,
    // decrements the session's available seats, and decrements the member's matching
    // membership balance.
    public interface IBookingService
    {
        // Books a member into a session after validating all rules. Atomically creates
        // the booking, decrements the session's AvailableSeats, and decrements the
        // matching membership's RemainingSessions. Throws on any rule violation.
        Task<BookingDto> BookAsync(
            CreateBookingRequest request, CancellationToken cancellationToken = default);

        // Cancels a booking. If cancelled more than 24h before the session starts, the
        // seat is freed and the session credit is returned to the membership. Within
        // 24h, the booking is cancelled but the session credit is forfeited.
        Task CancelAsync(Guid bookingId, CancellationToken cancellationToken = default);

        // Returns all bookings of a member, newest first.
        Task<IReadOnlyList<BookingDto>> GetByMemberAsync(
            Guid memberId, CancellationToken cancellationToken = default);
    }
}
