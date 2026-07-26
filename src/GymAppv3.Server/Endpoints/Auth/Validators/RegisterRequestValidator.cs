using FluentValidation;
using GymAppV3.Core.Validators;

namespace GymAppv3.Server.Endpoints.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Οι κωδικοί δεν ταιριάζουν.");
        RuleFor(x => x.Firstname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Lastname).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(x => x.BirthDate).NotEqual(default(DateOnly));
        RuleFor(x => x.MedicalNotes).MaximumLength(2048);
    }
}
