using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class UpdateClassCategoryCommandValidator : AbstractValidator<UpdateClassCategoryCommand>
{
    public UpdateClassCategoryCommandValidator()
    {
       RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
