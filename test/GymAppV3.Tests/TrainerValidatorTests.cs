using FluentAssertions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Validators;

namespace GymAppV3.Tests;

public class TrainerValidatorTests
{
    [Fact]
    public void CreateTrainerCommandValidator_NullSpecialties_ReturnsErrorsInsteadOfThrowing()
    {
        var command = new CreateTrainerCommand(null!, null!, null!, null, null, null!);

        var result = new CreateTrainerCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrainerCommand.SpecialtyCategoryIds));
    }

    [Fact]
    public void UpdateTrainerCommandValidator_NullSpecialties_ReturnsErrorsInsteadOfThrowing()
    {
        var command = new UpdateTrainerCommand(null!, null!, null!, null, null, null!);

        var result = new UpdateTrainerCommandValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTrainerCommand.SpecialtyCategoryIds));
    }
}
