using GymAppV3.Core.Commands;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.Interfaces;
using GymAppV3.Core.Queries.MembershipPackages;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GymAppv3.Server.Endpoints.MembershipPackage;

public static class MembershipPackageHandlers
{
    public static async Task<Ok<IReadOnlyList<MembershipPackageDto>>> GetAllAsync(
    IMembershipPackageQueryService queryService,
    CancellationToken cancellationToken)
    {
        var result = await queryService.GetAllAsync(new GetAllMembershipPackagesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<MembershipPackageDto>, NotFound>> GetByIdAsync(
        Guid id,
        IMembershipPackageQueryService queryService,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetByIdAsync(new GetMembershipPackageByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    public static async Task<Created<MembershipPackageDto>> CreateAsync(
        CreateMembershipPackageCommand command,
        IMembershipPackageCommandService commandService,
        CancellationToken cancellationToken)
    {
        var created = await commandService.CreateAsync(command, cancellationToken);
        return TypedResults.Created($"/api/membership-packages/{created.Id}", created);
    }

    public static async Task<NoContent> UpdateAsync(
        Guid id,
        UpdateMembershipPackageCommand command,
        IMembershipPackageCommandService commandService,
        CancellationToken cancellationToken)
    {

        await commandService.UpdateAsync(id, command, cancellationToken);
        return TypedResults.NoContent();

    }

    public static async Task<NoContent> DeleteAsync(
        Guid id,
        IMembershipPackageCommandService commandService,
        CancellationToken cancellationToken)
    {

        await commandService.DeleteAsync(id, cancellationToken);
        return TypedResults.NoContent();

    }
}
