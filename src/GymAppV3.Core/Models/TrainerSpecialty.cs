using GymAppV3.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Models
{
    public class TrainerSpecialty
    {
        // Reference to the trainer holding this specialty
        public Guid TrainerId { get; set; }

        // Navigation property to the Trainer entity
        public Trainer Trainer { get; set; } = null!;

        // The specialty/area of expertise
        public Specialty Specialty { get; set; }
    }
}
