using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Interfaces;

public interface IClassCategoryCommandService
{
    Task<ClassCategoryDto> CreateAsync(CreateClassCategoryCommand command, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateClassCategoryCommand command, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
