
namespace GymAppV3.Core.Models
{
    public class ClassRoom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string ClassRoomName { get; set; }
    }
}
