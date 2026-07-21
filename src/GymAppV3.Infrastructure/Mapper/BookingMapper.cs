using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers;

internal static class BookingMapper
{
    // SessionTitle and SessionStartsAt come from the ClassSession navigation, so a
    // client can render the booking list without a second lookup per row.
    public static readonly Expression<Func<Booking, BookingDto>> ToDto =
        b => new BookingDto(
            b.Id,
            b.MemberId,
            b.ClassSessionId,
            b.ClassSession.Title,
            b.ClassSession.StartsAt,
            b.Status.ToString(),
            b.BookedAt,
            b.CancelledAt);

    public static readonly Func<Booking, BookingDto> ToDtoCompiled = ToDto.Compile();
}
