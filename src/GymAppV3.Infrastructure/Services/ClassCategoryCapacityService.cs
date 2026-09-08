
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Enums;
using GymAppV3.Core.Interfaces;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

public class ClassCategoryCapacityService : IClassCategoryCapacityService
{
    // Supply is sampled over a rolling 4-week (28-day) window and averaged, rather
    // than a single week, so one atypical week (a cancelled class, a holiday)
    // doesn't distort the number. This assumes the timetable is a stable, repeating
    // weekly schedule — which is how this gym runs it.
    private const int SupplyWindowDays = 28;
    private const int SupplyWindowWeeks = SupplyWindowDays / 7;

    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;

    public ClassCategoryCapacityService(ApplicationDbContext context, IDateTimeProvider clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<(double WeeklySupply, double WeeklyDemand)> GetWeeklyCapacityAsync(
        Guid classCategoryId, CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var windowEnd = now.AddDays(SupplyWindowDays);

        // Supply: total bookable seats across every scheduled session occurrence of
        // this category in the next 4 weeks, averaged to a weekly figure. Counts
        // session OCCURRENCES × Capacity, not class-hours — a membership "session"
        // is one booking against one occurrence, regardless of how long that class
        // runs, so occurrences are the right unit here.
        var totalSeatSlots = await _context.ClassSessions
            .Where(s => s.ClassCategoryId == classCategoryId
                        && s.StartsAt >= now
                        && s.StartsAt < windowEnd)
            .SumAsync(s => (int?)s.Capacity, cancellationToken) ?? 0;

        var weeklySupply = totalSeatSlots / (double)SupplyWindowWeeks;

        // Demand: sum of the weekly session rate of every member currently holding
        // an active, non-expired membership in this category — regardless of which
        // specific package each one bought.
        var activeMemberships = await _context.Memberships
            .Where(m => m.MembershipPackage.ClassCategoryId == classCategoryId
                     && m.Status == MembershipStatus.Active
                     && m.EndDate > now)
            .Select(m => new { m.MembershipPackage.SessionsIncluded, m.MembershipPackage.DurationInDays })
            .ToListAsync(cancellationToken);

        var weeklyDemand = activeMemberships.Sum(m => GetWeeklyRate(m.SessionsIncluded, m.DurationInDays));

        return (weeklySupply, weeklyDemand);
    }

    public double GetWeeklyRate(int sessionsIncluded, int durationInDays)
    {
        if (durationInDays <= 0) return 0;

        var weeks = Math.Max(1, (int)Math.Round(durationInDays / 7.0, MidpointRounding.AwayFromZero));
        return sessionsIncluded / (double)weeks;
    }
}
