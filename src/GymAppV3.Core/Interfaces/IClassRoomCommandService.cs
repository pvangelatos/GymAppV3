using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IClassRoomCommandService
{
    Task<ClassRoomDto> CreateAsync(CreateClassRoomCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateClassRoomCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
