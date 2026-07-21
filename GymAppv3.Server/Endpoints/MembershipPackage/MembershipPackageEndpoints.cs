using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;

namespace GymAppv3.Server.Endpoints.MembershipPackage;

public static class MembershipPackageEndpoints
{
    public static IEndpointRouteBuilder MapMembershipPackageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/membership-packages")
            .WithTags("Membership Packages");

        group.MapGet("/", MembershipPackageHandlers.GetAllAsync)
            .WithName("GetMembershipPackages")
            .RequireAuthorization()
            .Produces<IReadOnlyList<MembershipPackageDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", MembershipPackageHandlers.GetByIdAsync)
            .WithName("GetMembershipPackageById")
            .RequireAuthorization()
            .Produces<MembershipPackageDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", MembershipPackageHandlers.CreateAsync)
            .WithName("CreateMembershipPackage")
            .RequireAuthorization("AdminOnly")
            .Accepts<CreateMembershipPackageCommand>("application/json")
            .Produces<MembershipPackageDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", MembershipPackageHandlers.UpdateAsync)
            .WithName("UpdateMembershipPackage")
            .RequireAuthorization("AdminOnly")
            .Accepts<UpdateMembershipPackageCommand>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", MembershipPackageHandlers.DeleteAsync)
            .WithName("DeleteMembershipPackage")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
