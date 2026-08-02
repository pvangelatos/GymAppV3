using System.Collections.Concurrent;
using System.Reflection;
using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Exceptions;
using GymAppV3.Infrastructure.Data;
using GymAppV3.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymAppv3.Server.Endpoints.Common;

/// <summary>
/// Endpoint filter for routes shaped /api/members/{memberId:guid}/...
/// Defense-in-depth on top of the service-level ownership checks: rejects
/// non-staff callers whose own member record doesn't match the route memberId,
/// and 400s early if a bound command's MemberId disagrees with the route —
/// instead of letting a silently-ignored route segment reach the service.
/// </summary>
public class MemberScopedFilter : IEndpointFilter
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> MemberIdPropertyCache = new();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.RouteValues.TryGetValue("memberId", out var raw) ||
            !Guid.TryParse(raw?.ToString(), out var routeMemberId))
        {
            throw new BusinessRuleException("Route is missing a valid memberId.");
        }

        var userContext = httpContext.RequestServices.GetRequiredService<IUserContext>();

        if (!userContext.IsStaff())
        {
            var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var isOwner = await dbContext.Members.AnyAsync(
                m => m.Id == routeMemberId && m.UserId == userContext.UserId,
                httpContext.RequestAborted);

            if (!isOwner)
                throw new ForbiddenException("You are not allowed to access this member's data.");
        }

        // Catch route/body MemberId mismatches early, before touching the service layer.
        foreach (var argument in context.Arguments)
        {
            if (argument is null) continue;

            var property = MemberIdPropertyCache.GetOrAdd(argument.GetType(),
                t => t.GetProperty("MemberId", typeof(Guid)));

            if (property is not null && (Guid)property.GetValue(argument)! != routeMemberId)
            {
                throw new BusinessRuleException("The MemberId in the request body does not match the route.");
            }
        }

        return await next(context);
    }
}
