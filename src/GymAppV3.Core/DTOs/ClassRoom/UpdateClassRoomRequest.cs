namespace GymAppV3.Core.DTOs.ClassRoom
{
    // Input for updating a room. Id comes from the route.
    public record UpdateClassRoomRequest(
        string ClassRoomName,
        int Capacity);
}
