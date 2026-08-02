namespace GymAppV3.Core.Commands;

public record ScheduleRecurringClassSessionCommand(
    string Title,
    Guid ClassCategoryId,
    DateTimeOffset StartsAt,
    int DurationInMinutes,
    int Capacity,
    Guid TrainerId,
    Guid ClassRoomId,
    int RepeatWeeks);
