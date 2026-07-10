using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace GymAppV3.Infrastructure.Data.Mappings;

/// <summary>
/// EF Core configuration for the Address entity
/// Defines database structure for physical location information
/// </summary>
public class AddressMap : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        // Map to "Addresses" table in "dbo" schema
        builder.ToTable("Addresses", "dbo");

        // Street name is required with max 512 characters
        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M512);

        // City/municipality is required with max 512 characters
        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M512);

        // State/region is required with max 512 characters
        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M512);

        // Postal code is required with max 64 characters (supports various formats)
        builder.Property(x => x.ZipCode)
            .IsRequired()
            .HasMaxLength(TextSizePresets.S64);

        // Country name is required with max 512 characters
        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(TextSizePresets.M512);
    }
}
