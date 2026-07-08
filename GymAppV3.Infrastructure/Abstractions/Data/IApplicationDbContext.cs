using GymAppV3.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Abstractions.Data
{
    public interface IApplicationDbContext
    {
        DbSet<MembershipPackage> MembershipPackages { get; }
        DbSet<Member> Members { get; }
        DbSet<Trainer> Trainers { get; }
        DbSet<ClassSession> ClassSessions { get; }
        DbSet<Membership> Memberships { get; }
        DbSet<Booking> Bookings { get; }
        DbSet<Payment> Payments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
