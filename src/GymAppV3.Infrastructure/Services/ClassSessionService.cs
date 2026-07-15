using GymAppV3.Core.Abstractions;
using GymAppV3.Core.DTOs.ClassSession;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services
{
    public class ClassSessionService : IClassSessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public ClassSessionService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }
        public async Task<ClassSessionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ClassSessions
                .Where(s => s.Id == id)
                .Select(s => new ClassSessionDto(
                    s.Id, s.Title, s.ClassCategoryId, s.ClassCategory.Name,
                    s.StartsAt, s.DurationInMinutes,
                    s.Capacity, s.AvailableSeats, s.TrainerId,
                    s.Trainer.Firstname + " " + s.Trainer.Lastname,
                    s.ClassRoomId, s.ClassRoom.ClassRoomName))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClassSessionDto>> GetUpcomingAsync(CancellationToken cancellationToken = default)
        {
            var now = _clock.UtcNow;

            // SQLite cannot translate DateTimeOffset comparisons, so we fetch first and
            // filter/order in memory. On SQL Server this comparison would run in the
            // database. (For a real app you'd narrow the fetch by a translatable column
            // if the session count grew large.)
            var sessions = await _context.ClassSessions
                .Where(s => s.StartsAt > now)
                .OrderBy(s => s.StartsAt)
                .Select(s => new ClassSessionDto(
                    s.Id, s.Title, s.ClassCategoryId, s.ClassCategory.Name,
                    s.StartsAt, s.DurationInMinutes,
                    s.Capacity, s.AvailableSeats, s.TrainerId,
                    s.Trainer.Firstname + " " + s.Trainer.Lastname,
                    s.ClassRoomId, s.ClassRoom.ClassRoomName))
                .ToListAsync(cancellationToken);

            return sessions;
        }

        public async Task<ClassSessionDto> ScheduleAsync(ScheduleClassSessionRequest request, CancellationToken cancellationToken = default)
        {
            // --- Rule 2: the session must start in the future ---
            // Uses the injected clock (not DateTimeOffset.UtcNow) so this is testable.
            if (request.StartsAt <= _clock.UtcNow)
                throw new BusinessRuleException("A session cannot be scheduled in the past.");

            // --- Rule 1 (part a): capacity must be positive ---
            if (request.Capacity <= 0)
                throw new BusinessRuleException("Capacity must be greater than zero.");

            // --- Rule 3: the trainer must exist (and not be soft-deleted) ---
            // The global query filter already excludes soft-deleted rows, so a simple
            // existence check is enough.
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.Id == request.TrainerId, cancellationToken) ??
                throw new NotFoundException(nameof(Trainer), request.TrainerId);

            // --- Rule 4: the room must exist ---
            var room = await _context.ClassRooms
                .FirstOrDefaultAsync(r => r.Id == request.ClassRoomId, cancellationToken) ??
                throw new NotFoundException(nameof(ClassRoom), request.ClassRoomId);

            // --- Rule 1 (part b): session capacity cannot exceed the room's physical capacity ---
            if (request.Capacity > room.Capacity)
                throw new BusinessRuleException(
                    $"Session capacity ({request.Capacity}) exceeds room capacity ({room.Capacity}).");

            // --- Rule 5: no overlapping session in the same room ---
            var newStart = request.StartsAt;
            var newEnd = request.StartsAt.AddMinutes(request.DurationInMinutes);

            var hasRoomSessionsConficts = await _context.ClassSessions
                .Where(s => s.ClassRoomId == request.ClassRoomId && s.StartsAt < newEnd && s.EndsAt > newStart)
                .AnyAsync(cancellationToken);

            if (hasRoomSessionsConficts)
                throw new BusinessRuleException("The room is already booked for an overlapping time slot.");

            // --- The category must exist ---
            var category = await _context.ClassCategories
                .FirstOrDefaultAsync(c => c.Id == request.ClassCategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);

            // All rules passed - create the session
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

            return MapToDto(session, trainer, room, category);
        }

        // Local mapping helper for the create path, where we already have the loaded
        // trainer and room in memory and don't need a second query.
        private static ClassSessionDto MapToDto(ClassSession session, Trainer trainer, 
            ClassRoom room, ClassCategory category) =>
            new(
                session.Id, session.Title, category.Id, category.Name, session.StartsAt,
                session.DurationInMinutes, session.Capacity, session.AvailableSeats, 
                session.TrainerId, trainer.Firstname + " " + trainer.Lastname,
                room.Id, room.ClassRoomName); 
    }
}
