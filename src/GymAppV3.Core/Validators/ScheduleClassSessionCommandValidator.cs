using FluentValidation;
using GymAppV3.Core.Commands;

namespace GymAppV3.Core.Validators;

public class ScheduleClassSessionCommandValidator : AbstractValidator<ScheduleClassSessionCommand>
{
    public ScheduleClassSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(512).WithMessage("Title cannot exceed 512 characters.");

        RuleFor(x => x.ClassCategoryId)
            .NotEmpty().WithMessage("ClassCategoryId is required.");

        RuleFor(x => x.TrainerId)
            .NotEmpty().WithMessage("TrainerId is required.");

        RuleFor(x => x.ClassRoomId)
            .NotEmpty().WithMessage("ClassRoomId is required.");

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than zero.")
            .LessThanOrEqualTo(480).WithMessage("Duration cannot exceed 8 hours.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than zero.");
    }
}
