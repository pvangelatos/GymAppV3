
using FluentValidation;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(256);
            
        RuleFor(x => x.City).NotEmpty().MaximumLength(64);

        RuleFor(x => x.State).NotEmpty().MaximumLength(64);

        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(32);

        RuleFor(x => x.Country).NotEmpty().MaximumLength(64);
    }
}
