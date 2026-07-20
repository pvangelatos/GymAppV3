using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Models;
using GymAppV3.Core.Queries.ClassSessions;
using GymAppV3.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Infrastructure.Services
{
    public class ClassSessionService : IClassSessionCommandService, IClassSessionQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _clock;

        public ClassSessionService(ApplicationDbContext context, IDateTimeProvider clock)
        {
            _context = context;
            _clock = clock;
        }
        public async Task<ClassSessionDto?> GetClassSessionByIdAsync(GetClassSessionByIdQuery query, CancellationToken cancellationToken = default)
        {
            return await _context.ClassSessions
                .Where(s => s.Id == query.Id)
                .Select(ObjectMapper.ClassSession.ToDto)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ClassSessionDto>> GetUpcomingAsync(GetUpcomingClassSessionsQuery query, CancellationToken cancellationToken = default)
        {
            var now = _clock.UtcNow;

            // Fetch upcoming sessions starting in the future, projected straight to DTOs via ObjectMapper
            return await _context.ClassSessions
                .Where(s => s.StartsAt > now)
                .OrderBy(s => s.StartsAt)
                .Select(ObjectMapper.ClassSession.ToDto)
                .ToListAsync(cancellationToken);

        }

        public async Task<ClassSessionDto> ScheduleAsync(ScheduleClassSessionCommand request, CancellationToken cancellationToken = default)
        {
            // --- Business Rule: Future scheduling validation ---
            if (request.StartsAt <= _clock.UtcNow)
                throw new BusinessRuleException("A session cannot be scheduled in the past.");

            // ---Business Rule: Capacity validation ---
            if (request.Capacity <= 0)
                throw new BusinessRuleException("Capacity must be greater than zero.");

            // --- Business Rule: Trainer existence check ---
            // Global query filters automatically exclude soft-deleted trainers
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.Id == request.TrainerId, cancellationToken) ??
                throw new NotFoundException(nameof(Trainer), request.TrainerId);

            // --- Business Rule: Room existence & physical capacity check ---
            var room = await _context.ClassRooms
                .FirstOrDefaultAsync(r => r.Id == request.ClassRoomId, cancellationToken) ??
                throw new NotFoundException(nameof(ClassRoom), request.ClassRoomId);

            if (request.Capacity > room.Capacity)
                throw new BusinessRuleException(
                    $"Session capacity ({request.Capacity}) exceeds room capacity ({room.Capacity}).");

            // --- Business Rule: Category existence check ---
            var category = await _context.ClassCategories
                .FirstOrDefaultAsync(c => c.Id == request.ClassCategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(ClassCategory), request.ClassCategoryId);

            // --- Business Rule: Room schedule overlap check ---
            var newStart = request.StartsAt;
            var newEnd = request.StartsAt.AddMinutes(request.DurationInMinutes);

            var hasRoomSessionsConficts = await _context.ClassSessions
                .Where(s => s.ClassRoomId == request.ClassRoomId && s.StartsAt < newEnd && s.EndsAt > newStart)
                .AnyAsync(cancellationToken);

            if (hasRoomSessionsConficts)
                throw new BusinessRuleException("The room is already booked for an overlapping time slot.");

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

        
    }
}
