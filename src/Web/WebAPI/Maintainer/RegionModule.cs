
using Application.Maintainer.Regions.Commands.CreateRegion;
using Application.Maintainer.Regions.Commands.ToggleRegion;
using Application.Maintainer.Regions.Commands.UpdateRegion;
using Application.Maintainer.Regions.Queries.GetRegionById;
using Application.Maintainer.Regions.Queries.GetRegions;
using Domain.Common;

namespace WebAPI.Maintainer;

public class RegionModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/regions")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetRegions);
        group.MapGet("{id:guid}", GetRegionById);
        group.MapPost("", CreateRegion);
        group.MapPut("{id:guid}", UpdateRegion);
        group.MapDelete("{id:guid}", ToggleRegion);
    }


    private static async Task<IResult> GetRegions(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetRegions()));

    private static async Task<IResult> GetRegionById(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new GetRegionById(id)));

    private static async Task<IResult> CreateRegion(IRequestDispatcher sender, CreateRegion command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateRegion(IRequestDispatcher sender, Guid id, UpdateRegion command)
    {
        if (id != command.RegionId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la de la región {command.RegionId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleRegion(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new ToggleRegion(id)));

}
