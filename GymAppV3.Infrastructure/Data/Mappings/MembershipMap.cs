using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Data.Mappings
{
    public class MembershipMap : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
            builder.ToTable("Memberships", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.PricePaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.RemainingSessions)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(TextSizePresets.XS32);


            builder.HasOne(x => x.Member)
                .WithMany()
                .HasForeignKey(x => x.Member)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MembershipPackage)
                .WithMany()
                .HasForeignKey(x => x.MembershipPackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
