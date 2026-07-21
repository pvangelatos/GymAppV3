using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;
using System;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure.Mappers
{
    internal static class MembershipMapper
    {
        // PackageName comes from the MembershipPackage navigation. Status is an enum on
        // the entity but a string on the DTO — .ToString() inside the Expression is
        // translated by EF Core, so this still runs as a single SQL SELECT.
        public static readonly Expression<Func<Membership, MembershipDto>> ToDto =
            m => new MembershipDto(
                m.Id,
                m.MemberId,
                m.MembershipPackageId,
                m.MembershipPackage.Name,
                m.PricePaid,
                m.StartDate,
                m.EndDate,
                m.RemainingSessions,
                m.Status.ToString());

        public static readonly Func<Membership, MembershipDto> ToDtoCompiled = ToDto.Compile();
    }
}
