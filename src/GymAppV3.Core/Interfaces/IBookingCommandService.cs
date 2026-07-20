using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IBookingCommandService
{
    Task<BookingDto> BookAsync(CreateBookingCommand command, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
