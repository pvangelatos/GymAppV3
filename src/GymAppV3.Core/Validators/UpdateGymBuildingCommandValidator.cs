using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class UpdateGymBuildingCommandValidator : AbstractValidator<UpdateGymBuildingCommand>
{
    public UpdateGymBuildingCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
