namespace GymAppV3.Infrastructure.Abstractions;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
