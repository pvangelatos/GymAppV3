using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class ScheduleClassSessionCommandValidator : AbstractValidator<ScheduleClassSessionCommand>
{
    public ScheduleClassSessionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);

        RuleFor(x => x.ClassCategoryId).NotEmpty();

        RuleFor(x => x.TrainerId).NotEmpty();

        RuleFor(x => x.ClassRoomId).NotEmpty();

        RuleFor(x => x.DurationInMinutes).GreaterThan(0).LessThanOrEqualTo(480);

        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
