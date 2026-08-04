using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(x => x.Firstname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Lastname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(x => x.BirthDate).NotEqual(default(DateOnly));
        RuleFor(x => x.MedicalNotes).MaximumLength(1024);
    }
}
