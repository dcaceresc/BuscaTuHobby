using Domain.Common;
using Application.Maintainer.Communes.Queries.GetCommuneById;
using Application.Maintainer.Communes.Commands.CreateCommune;
using Application.Maintainer.Communes.Commands.UpdateCommune;
using Application.Maintainer.Communes.Commands.ToggleCommune;
using Application.Maintainer.Communes.Queries.GetCommunes;

namespace WebAPI.Maintainer;

public class CommuneModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/communes")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetCommunes);
        group.MapGet("{id:guid}", GetCommuneById);
        group.MapPost("", CreateCommune);
        group.MapPut("{id:guid}", UpdateCommune);
        group.MapDelete("{id:guid}", ToggleCommune);
    }

    private static async Task<IResult> GetCommunes(ISender sender) => Results.Ok(await sender.Send(new GetCommunes()));

    private static async Task<IResult> GetCommuneById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetCommuneById(id)));

    private static async Task<IResult> CreateCommune(ISender sender, CreateCommune command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateCommune(ISender sender, Guid id, UpdateCommune command)
    {
        if (id != command.CommuneId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la communa con Id {command.CommuneId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleCommune(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleCommune(id)));


}
