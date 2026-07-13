using GymAppV3.Core.DTOs.ClassCategory;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    // CRUD service for class categories (e.g. "Pilates Reformer", "Yoga"). Categories
    // are a lookup table so a gym can manage them at runtime; sessions, packages and
    // trainer specialties all reference these. The implementation uses the DbContext
    // directly — no repository layer.
    public interface IClassCategoryService
    {
        // Returns all active (non-soft-deleted) categories.
        Task<IReadOnlyList<ClassCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

        // Returns a single category, or null if it does not exist.
        Task<ClassCategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Creates a new category and returns the created record.
        Task<ClassCategoryDto> CreateAsync(CreateClassCategoryRequest request, CancellationToken cancellationToken = default);

        // Updates an existing category. Throws NotFoundException if it does not exist.
        Task UpdateAsync(Guid id, UpdateClassCategoryRequest request, CancellationToken cancellationToken = default);

        // Soft-deletes a category (the interceptor converts the delete to an update).
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
