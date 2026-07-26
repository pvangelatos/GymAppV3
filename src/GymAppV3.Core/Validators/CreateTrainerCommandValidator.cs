using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class CreateTrainerCommandValidator : AbstractValidator<CreateTrainerCommand>
{
    public CreateTrainerCommandValidator()
    {
        RuleFor(x => x.Firstname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Lastname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Bio).MaximumLength(2048);
        RuleFor(x => x.SpecialtyCategoryIds)
            .NotNull()
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Specialty category IDs must not contain empty GUIDs.");
    }
}
