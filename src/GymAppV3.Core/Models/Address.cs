namespace GymAppV3.Core.Models;

public class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Country { get; set; }
}
