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

        // The category the trainer is qualified in (replaces the old Specialty enum).
        public Guid ClassCategoryId { get; set; }
        public ClassCategory ClassCategory { get; set; } = null!;
    }
}
