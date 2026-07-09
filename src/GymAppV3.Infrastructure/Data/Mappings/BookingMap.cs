using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAppV3.Infrastructure.Data.Mappings;

public class BookingMap : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(TextSizePresets.XS32);

        builder.Property(x => x.BookedAt)
            .IsRequired();

        builder.Property(x => x.CancelledAt);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.Member)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClassSession)
            .WithMany()
            .HasForeignKey(x => x.ClassSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
