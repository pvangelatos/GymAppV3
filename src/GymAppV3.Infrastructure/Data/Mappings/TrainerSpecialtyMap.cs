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
        builder.HasKey(x => new { x.TrainerId, x.ClassCategoryId });

        builder.HasOne(x => x.ClassCategory)
            .WithMany()
            .HasForeignKey(x => x.ClassCategoryId)
            .OnDelete(DeleteBehavior.Restrict);     // don't cascade-delete categories

        // One trainer has many specialties.
        builder.HasOne(x => x.Trainer)
            .WithMany(t => t.Specialties)          
            .HasForeignKey(x => x.TrainerId)
            .OnDelete(DeleteBehavior.Cascade);     // remove trainer -> remove their links
    }
}
