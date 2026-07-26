
using FluentValidation;
using GymAppV3.Core.DTOs;

namespace GymAppV3.Core.Validators;

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(256);
            
        RuleFor(x => x.City).NotEmpty().MaximumLength(128);

        RuleFor(x => x.State).NotEmpty().MaximumLength(128);

        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Country).NotEmpty().MaximumLength(128);
    }
}
