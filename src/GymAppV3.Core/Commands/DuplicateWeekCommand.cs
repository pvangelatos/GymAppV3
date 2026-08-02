namespace GymAppV3.Core.Command;

public record DuplicateWeekCommand(
    DateTimeOffset SourceWeekStart,
    DateTimeOffset SourceWeekEnd,
    int RepeatWeeks);
