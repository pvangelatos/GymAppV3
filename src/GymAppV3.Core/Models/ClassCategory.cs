using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Models
{

    // A category of class/activity (e.g. "Pilates Reformer", "Yoga"). Modelled as a
    // lookup table rather than an enum, so each gym can add its own categories at
    // runtime without a code change or deploy. Sessions, packages, and trainer
    // specialties all reference this.
    public class ClassCategory : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Display name, e.g. "Pilates Reformer". Unique so the same category can't be
        // created twice.
        public required string Name { get; set; }
    }
}
