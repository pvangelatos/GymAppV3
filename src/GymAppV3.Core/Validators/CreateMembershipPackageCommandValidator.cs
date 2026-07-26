using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators; 

public class CreateMembershipPackageCommandValidator : AbstractValidator<CreateMembershipPackageCommand>
{
    public CreateMembershipPackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.DurationInDays).GreaterThan(0).LessThanOrEqualTo(365);
        RuleFor(x => x.SessionsIncluded).GreaterThan(0);
        RuleFor(x => x.ClassCategoryId).NotEmpty();
    }
}
