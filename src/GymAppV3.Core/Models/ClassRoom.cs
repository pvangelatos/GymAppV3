
namespace GymAppV3.Core.Models;

public class ClassRoom : AuditableEntity
{
    // Unique identifier for the classroom
    public Guid Id { get; set; } = Guid.NewGuid();

    // Name or identifier of the classroom (e.g., "Room A", "Yoga Studio")
    public required string ClassRoomName { get; set; }

    // Physical capacity of the room (e.g., number of Reformer beds).
    // A session scheduled in this room must not exceed this limit.
    public int Capacity { get; set; }

    // The building this room belongs to.
    public Guid GymBuildingId { get; set; }

    // Navigation property to the owning building.
    public GymBuilding GymBuilding { get; set; } = null!;
}
