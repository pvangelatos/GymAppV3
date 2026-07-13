using GymAppV3.Core.DTOs.ClassSession;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Interfaces
{
    // Use-case service for scheduling and reading class sessions. Unlike the CRUD
    // services, ScheduleAsync enforces business rules (capacity, timing, existence,
    // room conflicts) rather than blindly inserting.
    public interface IClassSessionService
    {
        // Schedules a new session after validating all business rules. Returns the
        // created session. Throws if any rule is violated.
        Task<ClassSessionDto> ScheduleAsync(
            ScheduleClassSessionRequest request, CancellationToken cancellationToken = default);

        // Returns a single session by id, or null if not found.
        Task<ClassSessionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Returns all sessions that start in the future, ordered by start time.
        Task<IReadOnlyList<ClassSessionDto>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    }
}

