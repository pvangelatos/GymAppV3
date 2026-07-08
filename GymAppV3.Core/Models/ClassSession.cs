using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Models
{
    public class ClassSession : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public required string Title { get; set; }
        public DateTimeOffset StartsAt { get; set; }
        public int DurationInMinutes { get; set; }
        public int Capacity {  get; set; }

        public Guid TrainerId { get; set; }
        public Trainer Trainer { get; set; } = null!;
    }
}
