namespace GymAppV3.Infrastructure.Data.Common;

/// <summary>
/// Predefined text field size limits for database column constraints
/// Used to ensure consistent maximum lengths across string properties in the database
/// </summary>
public static class TextSizePresets
{
    // Extra small - for short codes or identifiers (32 characters)
    public const int XS32 = 32;

    // Small - for names, titles (64 characters)
    public const int S64 = 64;

    // Medium - for descriptions, short content (256 characters)
    public const int M256 = 256;

    // Medium-Large - for longer descriptions (512 characters)
    public const int M512 = 512;

    // Large - for detailed content, bios, notes (1024 characters)
    public const int L1024 = 1024;

    // Extra Large - for extensive text content (4000 characters)
    public const int XL4000 = 4000;
}
