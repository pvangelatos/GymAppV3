using GymAppV3.Core.DTOs;
using GymAppV3.Core.Queries.ClassRooms;

namespace GymAppV3.Core.Interfaces;

public interface IClassRoomQueryService
{
    Task<IReadOnlyList<ClassRoomDto>> GetAllAsync(GetAllClassRoomsQuery query, CancellationToken cancellationToken = default);
    Task<ClassRoomDto?> GetByIdAsync(GetClassRoomByIdQuery query, CancellationToken cancellationToken = default);
}
