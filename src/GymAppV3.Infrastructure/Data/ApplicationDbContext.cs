using System.Linq.Expressions;
using GymAppV3.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Data;

/// <summary>
/// The EF Core database context. Registered in DI and injected directly into
/// application services.
/// </summary>
public class ApplicationDbContext : DbContext
{
    // Options (connection string, provider, interceptors) are injected from the
    // outside via DI, keeping this context free of hard-coded configuration.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Each DbSet is a table as seen from code. Expression-bodied Set<T>() avoids
    // nullable warnings that the classic { get; set; } form produces.
    public DbSet<MembershipPackage> MembershipPackages => Set<MembershipPackage>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<GymBuilding> GymBuildings => Set<GymBuilding>();
    public DbSet<TrainerSpecialty> TrainerSpecialties => Set<TrainerSpecialty>();
    public DbSet<ClassCategory> ClassCategories => Set<ClassCategory>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scan this assembly and apply every IEntityTypeConfiguration<T> automatically
        // (all the *Map classes in Data/Mappings). No need to register each by hand.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Apply soft-delete configuration to every entity deriving from AuditableEntity.
        // Done centrally via reflection so the rule lives in one place instead of being
        // repeated in each mapping — a single source of truth.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            // Build the lambda "(TEntity e) => !e.IsDeleted" dynamically, because the
            // entity type is only known at runtime inside this loop.
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
            var notDeleted = Expression.Not(isDeletedProperty);
            var filterLambda = Expression.Lambda(notDeleted, parameter);

            // Global query filter: soft-deleted rows are hidden from every query on this
            // entity unless explicitly bypassed with IgnoreQueryFilters().
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterLambda);

            // Filtered index supporting the query filter. Without it, each query scans
            // deleted rows only to discard them.
            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(nameof(AuditableEntity.IsDeleted))
                .HasFilter("[IsDeleted] = 0");

            // Length constraint for the DeletedBy audit column.
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.DeletedBy))
                .HasMaxLength(64);
        }

        // RowVersion (optimistic concurrency) is configured per provider:
        //  - SQL Server has a native rowversion type that the database auto-manages.
        //  - SQLite has no such type, so in tests we treat it as an ordinary column with
        //    a default, and rely on manual concurrency handling if ever needed there.
        var isSqlServer = Database.IsSqlServer();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
                continue;

            var rowVersionProperty = entityType.FindProperty("RowVersion");
            if (rowVersionProperty is null)
                continue;

            if (isSqlServer)
            {
                // Native SQL Server rowversion — database-generated concurrency token.
                modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .IsRowVersion();

                modelBuilder.Entity<Payment>().Property(p => p.Amount).HasConversion<double>();
                modelBuilder.Entity<Payment>().Property(p => p.NetAmount).HasConversion<double>();
            }
            else
            {
                // SQLite (tests): plain byte[] column with a default, so inserts don't
                // violate NOT NULL. Not a real concurrency token here, which is fine —
                // concurrency behaviour is verified against SQL Server, not SQLite.
                modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .HasDefaultValue(Array.Empty<byte>());
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}