using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAppV3.Infrastructure.Data.Mappings;

internal class MembershipPackageMap : IEntityTypeConfiguration<MembershipPackage>
{
    public void Configure(EntityTypeBuilder<MembershipPackage> builder)
    {
        // Configure table name.
        builder.ToTable("MembershipPackages", "dbo");

        // Configure primary key.
        builder.HasKey(x => x.Id);

        // Id comes from (Guid.NewGuid()), not from Db.
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // Configure columns.
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M512);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DurationInDays)
            .IsRequired();

        builder.Property(x => x.SessionsIncluded)
            .IsRequired();
    }
}
