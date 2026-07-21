using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers
{
    internal static class ClassSessionMapper
    {
        // Three navigations in one projection (Trainer, ClassRoom, ClassCategory).
        // EF Core still generates a single SELECT with the necessary JOINs — nothing
        // here causes N+1 queries as long as this Expression is used with .Select(),
        // not applied after materializing the entities.
        public static readonly Expression<Func<ClassSession, ClassSessionDto>> ToDto =
            s => new ClassSessionDto(
                s.Id,
                s.Title,
                s.ClassCategoryId,
                s.ClassCategory.Name,
                s.StartsAt,
                s.DurationInMinutes,
                s.Capacity,
                s.AvailableSeats,
                s.TrainerId,
                s.Trainer.Firstname + " " + s.Trainer.Lastname,
                s.ClassRoomId,
                s.ClassRoom.ClassRoomName);

        public static readonly Func<ClassSession, ClassSessionDto> ToDtoCompiled = ToDto.Compile();
    }
}
