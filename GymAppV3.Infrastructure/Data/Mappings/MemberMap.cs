using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Data.Mappings
{
    public class MemberMap : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            // Configure table name.
            builder.ToTable("Members", "dbo");

            // Configure primary key.
            builder.HasKey(x => x.Id);

            // Id comes from (Guid.NewGuid()), not from Db.
            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(TextSizePresets.S64);

            // Configure columns.
            builder.Property(x => x.Firstname)
                .IsRequired()
                .HasMaxLength(TextSizePresets.M512);

            builder.Property(x => x.Lastname)
                .IsRequired()
                .HasMaxLength(TextSizePresets.M512);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(TextSizePresets.M256);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.Phone)
                .HasMaxLength(TextSizePresets.S64);
                
        }
    }
}
