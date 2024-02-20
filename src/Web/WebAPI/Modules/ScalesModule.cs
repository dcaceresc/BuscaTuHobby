using Application.Maintainer.Scales.Commands.CreateScale;
using Application.Maintainer.Scales.Commands.ToggleScale;
using Application.Maintainer.Scales.Commands.UpdateScale;
using Application.Maintainer.Scales.Queries.GetScaleById;
using Application.Maintainer.Scales.Queries.GetScales;

namespace WebAPI.Modules;

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

    private static async Task<IResult> GetScales(ISender sender) => Results.Ok(await sender.Send(new GetScalesQuery()));

    private static async Task<IResult> GetScaleById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetScaleByIdQuery(id)));

    private static async Task<IResult> CreateScale(ISender sender, CreateScaleCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateScale(ISender sender, Guid id, UpdateScaleCommand command)
    {
        if (id != command.ScaleId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleScale(ISender sender, Guid id)
    {
        await sender.Send(new ToggleScaleCommand(id));

        return Results.NoContent();
    }
}
