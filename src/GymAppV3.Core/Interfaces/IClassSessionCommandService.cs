using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IClassSessionCommandService
{
    Task<ClassSessionDto> ScheduleAsync(ScheduleClassSessionCommand command, CancellationToken cancellationToken = default);
}
