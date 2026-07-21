using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers;

internal static class ClassCategoryMapper
{
    // EF Core uses the Expression to build a SQL SELECT with only the needed columns.
    public static readonly Expression<Func<ClassCategory, ClassCategoryDto>> ToDto =
        c => new ClassCategoryDto(c.Id, c.Name);

    // Compiled once at class load time — used when projecting an in-memory entity
    // (e.g. after an insert) so the mapping logic is never duplicated.
    public static readonly Func<ClassCategory, ClassCategoryDto> ToDtoCompiled = ToDto.Compile();
}
