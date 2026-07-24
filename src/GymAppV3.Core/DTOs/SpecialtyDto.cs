namespace GymAppV3.Core.DTOs;

// Category name travels with the id so clients don't need a second lookup.
public record SpecialtyDto(
   Guid ClassCategoryId,
   string ClassCategoryName);

