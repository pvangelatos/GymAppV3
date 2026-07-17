namespace GymAppV3.Core.Commands
{
    // Input for updating a room. Id comes from the route.
    public record UpdateClassRoomCommand(
        string ClassRoomName,
        int Capacity);
}
