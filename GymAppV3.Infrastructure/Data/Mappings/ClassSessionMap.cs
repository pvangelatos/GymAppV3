using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;

namespace GymAppV3.Infrastructure.Data.Mappings
{
    public class ClassSessionMap : IEntityTypeConfiguration<ClassSession>
    {
        public void Configure(EntityTypeBuilder<ClassSession> builder)
        {
            builder.ToTable("ClassSessions", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(TextSizePresets.M512);

            builder.Property(x => x.StartsAt)
                .IsRequired();

            builder.Property(x => x.DurationInMinutes)
                .IsRequired();

            builder.Property(x => x.DurationInMinutes)
                .IsRequired();

            builder.HasOne(x => x.Trainer)
                .WithMany()
                .HasForeignKey(x => x.Trainer)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
