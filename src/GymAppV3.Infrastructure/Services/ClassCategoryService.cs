using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services
{
    // CRUD implementation for class categories. The soft-delete global query filter
    // hides deleted rows automatically, so no method checks IsDeleted by hand.
    public class ClassCategoryService : IClassCategoryService
    {
        private readonly ApplicationDbContext _context;

        public ClassCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ClassCategoryDto> CreateAsync(CreateClassCategoryCommand request, CancellationToken cancellationToken = default)
        {
            var category = new ClassCategory { Name = request.Name };

            _context.ClassCategories.Add(category);
            // Save changes triggers the interceptor, which fills the audit fields.
            await _context.SaveChangesAsync(cancellationToken);

            return new ClassCategoryDto(category.Id, category.Name);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _context.ClassCategories
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ??
                throw new NotFoundException(nameof(ClassCategory), id);

            // Remove() marks the entity Deleted; the AuditableEntityInterceptor turns
            // that into a soft delete (UPDATE IsDeleted = 1) before it hits the database.
            _context.ClassCategories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClassCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // Project straight to the DTO so EF selects only the needed columns and
            // never materialises the full entity.
            return await _context.ClassCategories
                .Select(c => new ClassCategoryDto(c.Id, c.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<ClassCategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // FirstOrDefaultAsync (not FindAsync) so the soft-delete query filter is
            // respected — FindAsync would bypass it and could return a deleted row.
            return await _context.ClassCategories
                .Where(c => c.Id == id)
                .Select(c => new ClassCategoryDto(c.Id, c.Name))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(Guid id, UpdateClassCategoryCommand request, CancellationToken cancellationToken = default)
        {
            // Load the tracked entity (not a projection) so the change tracker can
            // generate the UPDATE from the mutation below.
            var category = await _context.ClassCategories
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken) ??
                throw new NotFoundException(nameof(ClassCategory), id);

            category.Name = request.Name;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
