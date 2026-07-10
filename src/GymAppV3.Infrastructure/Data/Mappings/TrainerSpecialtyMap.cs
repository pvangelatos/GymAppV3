using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAppV3.Infrastructure.Data.Mappings;

public class TrainerSpecialtyMap : IEntityTypeConfiguration<TrainerSpecialty>
{
    public void Configure(EntityTypeBuilder<TrainerSpecialty> builder)
    {
        builder.ToTable("TrainerSpecialties", "dbo");

        // Composite primary key — one trainer, one specialty.
        builder.HasKey(x => new { x.TrainerId, x.Specialty });

        builder.Property(x => x.Specialty)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(TextSizePresets.XS32);

        // One trainer has many specialties.
        builder.HasOne(x => x.Trainer)
            .WithMany(t => t.Specialties)          
            .HasForeignKey(x => x.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);     // delete trainer and his specialties
    }
}
