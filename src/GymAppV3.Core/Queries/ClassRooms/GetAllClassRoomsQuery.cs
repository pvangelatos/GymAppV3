using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Queries.ClassRooms;

public record GetAllClassRoomsQuery : IQuery<IReadOnlyList<ClassRoomDto>>;
