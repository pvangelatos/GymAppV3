using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Data.Mappings;

public class ClassCategoryMap : IEntityTypeConfiguration<ClassCategory>
{
    public void Configure(EntityTypeBuilder<ClassCategory> builder)
    {
        builder.ToTable("ClassCategories", "dbo");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M256);

        // Category names are unique among active rows (filtered for soft delete).
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
