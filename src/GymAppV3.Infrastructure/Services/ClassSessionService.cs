using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Command;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services;

public class ClassSessionService : IClassSessionCommandService, IClassSessionQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly IUserContext _userContext;
    private const int MaxRangeDays = 90; // The maximum range in days for fetching upcoming sessions.
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromDays(7);  // Default window for upcoming sessions if no range is specified.

    public ClassSessionService(ApplicationDbContext context, IDateTimeProvider clock, IUserContext userContext)
    {
        _context = context;
        _clock = clock;
        _userContext = userContext;
    }

    public async Task<ClassSessionDto?> GetClassSessionByIdAsync(GetClassSessionByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _context.ClassSessions
            .Where(s => s.Id == query.Id)
            .Select(ObjectMapper.ClassSession.ToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassSessionDto>> GetUpcomingAsync(
        GetUpcomingClassSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var from = query.From ?? _clock.UtcNow;
        var to = query.To ?? from.Add(DefaultWindow);

        if (to <= from)
            throw new BusinessRuleException("The 'To' date must be after the 'From' date.");

        if ((to - from).TotalDays > MaxRangeDays)
            throw new BusinessRuleException($"The date range between cannot exceed {MaxRangeDays} days.");

        // Fetch upcoming sessions starting in the future, projected straight to DTOs via ObjectMapper
        return await _context.ClassSessions
            .Where(s => s.StartsAt >= from && s.StartsAt < to)  // Only sessions starting within the specified range
            .OrderBy(s => s.StartsAt)
            .Select(ObjectMapper.ClassSession.ToDto)
            .ToListAsync(cancellationToken);

    }

    public async Task<ClassSessionDto> ScheduleAsync(ScheduleClassSessionCommand request, CancellationToken cancellationToken = default)
    {
        _userContext.EnsureIsStaff();

        // --- Business Rule: Future scheduling validation ---
        if (request.StartsAt <= _clock.UtcNow)
            throw new BusinessRuleException("A session cannot be scheduled in the past.");

        // --- Business Rule: Trainer existence check ---
        // Global query filters automatically exclude soft-deleted trainers
        if (!await _context.Trainers.AnyAsync(t => t.Id == request.TrainerId, cancellationToken))
            throw new NotFoundException(nameof(Trainer), request.TrainerId);

        // --- Business Rule: Room existence & physical capacity check ---
        var room = await _context.ClassRooms
            .FirstOrDefaultAsync(r => r.Id == request.ClassRoomId, cancellationToken) ??
            throw new NotFoundException(nameof(ClassRoom), request.ClassRoomId);

        if (request.Capacity > room.Capacity)
            throw new BusinessRuleException(
                $"Session capacity ({request.Capacity}) exceeds room capacity ({room.Capacity}).");

        // --- Business Rule: Category existence check ---
        if (!await _context.ClassCategories.AnyAsync(c => c.Id == request.ClassCategoryId, cancellationToken))
            throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);

        // --- Business Rule: Room schedule overlap check ---
        var newStart = request.StartsAt;
        var newEnd = request.StartsAt.AddMinutes(request.DurationInMinutes);

        var hasRoomConfict = await _context.ClassSessions
            .Where(s => s.ClassRoomId == request.ClassRoomId && s.StartsAt < newEnd && s.EndsAt > newStart)
            .AnyAsync(cancellationToken);

        if (hasRoomConfict)
            throw new BusinessRuleException("The room is already booked for an overlapping time slot.");

        // --- Business Rule: Trainer schedule overlap check ---
        var hasTrainerConflict = await _context.ClassSessions
            .Where(s => s.TrainerId == request.TrainerId && s.StartsAt < newEnd && s.EndsAt > newStart)
            .AnyAsync(cancellationToken);

        if (hasTrainerConflict)
            throw new BusinessRuleException("The trainer already has a session scheduled for an overlapping time slot.");

        // --- Construct entity ---
        var session = new ClassSession
        {
            Title = request.Title,
            ClassCategoryId = request.ClassCategoryId,
            StartsAt = request.StartsAt,
            EndsAt = newEnd,
            DurationInMinutes = request.DurationInMinutes,
            Capacity = request.Capacity,
            // No booking yet, so every seat is available.
            AvailableSeats = request.Capacity,
            TrainerId = request.TrainerId,
            ClassRoomId = request.ClassRoomId
        };

        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        // Return in-memory projection using the central ObjectMapper compiled delegate
        return ObjectMapper.ClassSession.ToDtoCompiled(session);
    }

    public async Task<IReadOnlyList<ClassSessionDto>> ScheduleRecurringAsync(ScheduleRecurringClassSessionCommand request, CancellationToken cancellationToken = default)
    {
        _userContext.EnsureIsStaff();

        if (request.RepeatWeeks < 1 || request.RepeatWeeks > 52)
            throw new BusinessRuleException("The number of repeat weeks must be between 1 and 52.");

        if (request.StartsAt <= _clock.UtcNow)
            throw new BusinessRuleException("A session cannot be scheduled in the past.");

        var trainer = await _context.Trainers
            .FirstOrDefaultAsync(t => t.Id == request.TrainerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Trainer), request.TrainerId);

        var room = await _context.ClassRooms
            .FirstOrDefaultAsync(r => r.Id == request.ClassRoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassRoom), request.ClassRoomId);

        if (request.Capacity > room.Capacity)
            throw new BusinessRuleException(
                $"Session capacity ({request.Capacity}) exceeds room capacity ({room.Capacity}).");

        var category = await _context.ClassCategories
            .FirstOrDefaultAsync(c => c.Id == request.ClassCategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);

        var occurrences = Enumerable.Range(0, request.RepeatWeeks)
            .Select(week => request.StartsAt.AddDays(week * 7))
            .ToList();

        // --- Validate ALL occurrences BEFORE creating any (all-or-nothing) ---
        var slots = occurrences
            .Select(start => new PlannedSlot(
                request.ClassRoomId,
                request.TrainerId,
                start,
                start.AddMinutes(request.DurationInMinutes),
                $"{start:dd/MM/yyyy HH:mm}"))
            .ToList();

        var conflicts = await FindConflictsAsync(slots, cancellationToken);

        if (conflicts.Count > 0)
            throw new BusinessRuleException(
                "Could not schedule all occurrences:\n" + string.Join("\n", conflicts));

        // --- All clear: create every occurrence, sharing a RecurrenceGroupId ---
        var recurrenceGroupId = request.RepeatWeeks > 1 ? Guid.NewGuid() : (Guid?)null;

        var sessions = occurrences.Select(occurrenceStart => new ClassSession
        {
            Title = request.Title,
            ClassCategoryId = request.ClassCategoryId,
            StartsAt = occurrenceStart,
            EndsAt = occurrenceStart.AddMinutes(request.DurationInMinutes),
            DurationInMinutes = request.DurationInMinutes,
            Capacity = request.Capacity,
            AvailableSeats = request.Capacity,
            TrainerId = request.TrainerId,
            ClassRoomId = request.ClassRoomId,
            RecurrenceGroupId = recurrenceGroupId
        }).ToList();

        _context.ClassSessions.AddRange(sessions);
        await _context.SaveChangesAsync(cancellationToken);

        return sessions.Select(ObjectMapper.ClassSession.ToDtoCompiled).ToList();
    }

    public async Task<IReadOnlyList<ClassSessionDto>> DuplicateWeekAsync(DuplicateWeekCommand request, CancellationToken cancellationToken = default)
    {
        _userContext.EnsureIsStaff();

        if (request.RepeatWeeks < 1 || request.RepeatWeeks > 52)
            throw new BusinessRuleException("The number of weeks must be between 1 and 52.");

        var sourceSessions = await _context.ClassSessions
            .AsNoTracking()
            .Where(s => s.StartsAt >= request.SourceWeekStart && s.StartsAt < request.SourceWeekEnd)
            .ToListAsync(cancellationToken);

        if (sourceSessions.Count == 0)
            throw new BusinessRuleException("There are no classes this week to duplicate.");

        var occurrences = new List<(ClassSession Source, DateTimeOffset NewStart)>();
        for (int week = 1; week <= request.RepeatWeeks; week++)
        {
            foreach (var s in sourceSessions)
            {
                occurrences.Add((s, s.StartsAt.AddDays(week * 7)));
            }
        }

        var now = _clock.UtcNow;
        var conflicts = new List<string>();
        var slots = new List<PlannedSlot>();

        foreach (var (source, newStart) in occurrences)
        {
            var label = $"{source.Title} @ {newStart:dd/MM/yyyy HH:mm}";

            if (newStart <= now)
            {
                conflicts.Add($"{label} — cannot be in the past.");
                continue;
            }

            slots.Add(new PlannedSlot(
                source.ClassRoomId,
                source.TrainerId,
                newStart,
                newStart.AddMinutes(source.DurationInMinutes),
                label));
        }

        conflicts.AddRange(await FindConflictsAsync(slots, cancellationToken));

        if (conflicts.Count > 0)
            throw new BusinessRuleException(
                "Could not duplicate the entire week:\n" + string.Join("\n", conflicts));

        var recurrenceGroupId = Guid.NewGuid();

        var newSessions = occurrences.Select(o => new ClassSession
        {
            Title = o.Source.Title,
            ClassCategoryId = o.Source.ClassCategoryId,
            StartsAt = o.NewStart,
            EndsAt = o.NewStart.AddMinutes(o.Source.DurationInMinutes),
            DurationInMinutes = o.Source.DurationInMinutes,
            Capacity = o.Source.Capacity,
            AvailableSeats = o.Source.Capacity,
            TrainerId = o.Source.TrainerId,
            ClassRoomId = o.Source.ClassRoomId,
            RecurrenceGroupId = recurrenceGroupId
        }).ToList();

        _context.ClassSessions.AddRange(newSessions);
        await _context.SaveChangesAsync(cancellationToken);

        return newSessions.Select(ObjectMapper.ClassSession.ToDtoCompiled).ToList();
    }

    // A session we are about to create, described just enough to check for clashes.
    private readonly record struct PlannedSlot(
        Guid RoomId, Guid TrainerId, DateTimeOffset Start, DateTimeOffset End, string Label);

    // Checks all planned slots against existing sessions with ONE database round-trip,
    // instead of two queries per slot.
    private async Task<List<string>> FindConflictsAsync(
        IReadOnlyList<PlannedSlot> slots, CancellationToken cancellationToken)
    {
        if (slots.Count == 0)
            return [];

        var from = slots.Min(s => s.Start);
        var to = slots.Max(s => s.End);
        var roomIds = slots.Select(s => s.RoomId).Distinct().ToList();
        var trainerIds = slots.Select(s => s.TrainerId).Distinct().ToList();

        var existing = await _context.ClassSessions
            .AsNoTracking()
            .Where(s => s.StartsAt < to && s.EndsAt > from
                     && (roomIds.Contains(s.ClassRoomId) || trainerIds.Contains(s.TrainerId)))
            .Select(s => new { s.ClassRoomId, s.TrainerId, s.StartsAt, s.EndsAt })
            .ToListAsync(cancellationToken);

        var conflicts = new List<string>();
        foreach (var slot in slots)
        {
            // Same half-open overlap rule as before: touching (end == start) is allowed.
            var overlapping = existing.Where(e => e.StartsAt < slot.End && e.EndsAt > slot.Start).ToList();

            if (overlapping.Any(e => e.ClassRoomId == slot.RoomId))
                conflicts.Add($"{slot.Label} — the room is already booked.");
            if (overlapping.Any(e => e.TrainerId == slot.TrainerId))
                conflicts.Add($"{slot.Label} — the trainer already has a session.");
        }

        return conflicts;
    }
}
