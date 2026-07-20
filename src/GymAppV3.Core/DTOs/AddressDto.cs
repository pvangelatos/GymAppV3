

namespace GymAppV3.Core.DTOs
{
    // Flat representation of the owned Address value object. Used inside building DTOs
    // so clients see a nested address without any entity identity leaking through.
    public record AddressDto(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country);
}
