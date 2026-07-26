using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class UpdateClassRoomCommandValidator : AbstractValidator<UpdateClassRoomCommand>
{
    public UpdateClassRoomCommandValidator()
    {
        RuleFor(x => x.ClassRoomName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(100);
       
    }
}
