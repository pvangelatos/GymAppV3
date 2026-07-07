using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Models
{
    public class MembershipPackage : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public int SessionsIncluded { get; set; }
    }
}
