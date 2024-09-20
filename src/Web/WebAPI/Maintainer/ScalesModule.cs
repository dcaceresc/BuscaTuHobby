using Application.Maintainer.Scales.Commands.CreateScale;
using Application.Maintainer.Scales.Commands.ToggleScale;
using Application.Maintainer.Scales.Commands.UpdateScale;
using Application.Maintainer.Scales.Queries.GetScaleById;
using Application.Maintainer.Scales.Queries.GetScales;
using Domain.Common;

namespace WebAPI.Maintainer;

public class ScalesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/scales").RequireAuthorization();

        group.MapGet("", GetScales);
        group.MapGet("{id:guid}", GetScaleById);
        group.MapPost("", CreateScale);
        group.MapPut("{id:guid}", UpdateScale);
        group.MapDelete("{id:guid}", ToggleScale);

    }

    private static async Task<IResult> GetScales(ISender sender) => Results.Ok(await sender.Send(new GetScales()));

    private static async Task<IResult> GetScaleById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetScaleById(id)));

    private static async Task<IResult> CreateScale(ISender sender, CreateScale command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateScale(ISender sender, Guid id, UpdateScale command)
    {
        if (id != command.ScaleId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la escala {command.ScaleId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleScale(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleScale(id)));
}
