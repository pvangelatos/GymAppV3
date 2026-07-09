using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Models
{
    public class ClassRoom
    {
        public Guid Id { get; set; } = new Guid();
        public required string ClassRoomName { get; set; }
    }
}
