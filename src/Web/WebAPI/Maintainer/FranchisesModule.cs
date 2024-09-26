using Application.Maintainer.Franchises.Commands.CreateFranchise;
using Application.Maintainer.Franchises.Commands.ToggleFranchise;
using Application.Maintainer.Franchises.Commands.UpdateFranchise;
using Application.Maintainer.Franchises.Queries.GetFranchiseById;
using Application.Maintainer.Franchises.Queries.GetFranchises;
using Domain.Common;

namespace WebAPI.Maintainer;

public class FranchisesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/franchises")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetFranchises);
        group.MapGet("{id:guid}", GetFranchiseById);
        group.MapPost("", CreateFranchise);
        group.MapPut("{id:guid}", UpdateFranchise);
        group.MapDelete("{id:guid}", ToggleFranchise);
    }

    private static async Task<IResult> GetFranchises(ISender sender) => Results.Ok(await sender.Send(new GetFranchises()));

    private static async Task<IResult> GetFranchiseById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetFranchiseById(id)));

    private static async Task<IResult> CreateFranchise(ISender sender, CreateFranchise command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateFranchise(ISender sender, Guid id, UpdateFranchise command)
    {
        if (id != command.FranchiseId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la de la franquicia {command.FranchiseId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleFranchise(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleFranchise(id)));
}
