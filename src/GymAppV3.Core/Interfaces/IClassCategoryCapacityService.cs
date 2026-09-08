namespace GymAppV3.Core.Interfaces;

// Computes membership "seat" capacity for a class category purely from live data:
// the actual scheduled sessions and the actually-active memberships. Nothing here
// is a stored counter, so it's always in sync — a class added to the timetable
// raises supply on the next call, an expired membership drops out of demand on
// the next call, with no maintenance code required anywhere else.
public interface IClassCategoryCapacityService
{
    Task<(double WeeklySupply, double WeeklyDemand)> GetWeeklyCapacityAsync(
        Guid classCategoryId, CancellationToken cancellationToken = default);

    // Weekly session rate of a package (SessionsIncluded normalised to a 7-day
    // week). Rounds the package's DurationInDays to the nearest whole week first,
    // so a standard 30-day/"monthly" package reads as 4 weeks — matching how these
    // packages are actually sold ("N sessions per month" = "N/4 per week").
    double GetWeeklyRate(int sessionsIncluded, int durationInDays);
}
