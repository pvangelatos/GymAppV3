using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class CreateClassCategoryCommandValidator : AbstractValidator<CreateClassCategoryCommand>
{
    public CreateClassCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        
    }
}
