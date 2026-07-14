using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymAppV3.Infrastructure.Data.Mappings;

/// <summary>
/// EF Core configuration for the Payment entity
/// Defines database structure for payment transactions
/// </summary>
public class PaymentMap : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Map to "Payments" table in "dbo" schema
        builder.ToTable("Payments", "dbo");

        // Define primary key
        builder.HasKey(x => x.Id);

        // Payment amount - required, stored as decimal(18,2) for financial precision (up to 18 digits, 2 decimals)
        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // VAT rate as a fraction, e.g. 0.2400. Four decimals is plenty for a percentage.
        builder.Property(x => x.VatRate)
            .IsRequired()
            .HasColumnType("decimal(5,4)");

        // Payment method (enum converted to string) - required with max 32 characters (e.g., "Cash", "Card", "BankTransfer")
        builder.Property(x => x.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(TextSizePresets.XS32);

        // Timestamp when payment was processed - required
        builder.Property(x => x.PaidAt)
            .IsRequired();

        // Relationship to Member - required, cascade delete prevented
        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Membership - optional (payment may not be tied to a specific membership), cascade delete prevented
        builder.HasOne(x => x.Membership)
            .WithMany()
            .HasForeignKey(x => x.MembershipId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
