using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure
{
    internal static class ObjectMapper
    {
        // EF Core uses the Expression to build a SQL SELECT with only the needed columns.
        private static readonly Expression<Func<ClassRoom, ClassRoomDto>> ToDto =
            r => new ClassRoomDto(r.Id, r.ClassRoomName, r.Capacity, r.GymBuildingId);

        // Compiled once at class load time — used when projecting an in-memory entity
        // (e.g. after an insert) so the mapping logic is never duplicated.
        public static readonly Func<ClassRoom, ClassRoomDto> ToDtoCompiled = ToDto.Compile();
    }
}
