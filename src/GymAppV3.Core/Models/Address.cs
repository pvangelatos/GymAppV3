namespace GymAppV3.Core.Models;

public class Address
{
    // Street address (e.g., "123 Main Street")
    public required string Street { get; set; }

    // City or municipality where the gym is located
    public required string City { get; set; }

    // State or region (e.g., "Attica", "California")
    public required string State { get; set; }

    // Postal/ZIP code for the address
    public required string ZipCode { get; set; }

    // Country name
    public required string Country { get; set; }

}
