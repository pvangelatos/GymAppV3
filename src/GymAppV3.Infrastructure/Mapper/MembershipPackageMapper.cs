using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers
{
    internal static class MembershipPackageMapper
    {
        // ClassCategoryName comes from the ClassCategory navigation — EF Core turns this
        // into a JOIN rather than a separate round-trip.
        public static readonly Expression<Func<MembershipPackage, MembershipPackageDto>> ToDto =
            p => new MembershipPackageDto(
                p.Id,
                p.Name,
                p.Price,
                p.DurationInDays,
                p.SessionsIncluded,
                p.ClassCategoryId,
                p.ClassCategory.Name);

        public static readonly Func<MembershipPackage, MembershipPackageDto> ToDtoCompiled = ToDto.Compile();
    }
}
