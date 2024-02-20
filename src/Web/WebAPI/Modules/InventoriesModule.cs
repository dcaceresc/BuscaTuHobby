
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.ToggleInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Application.Maintainer.Inventories.Queries.GetInventories;
using Application.Maintainer.Inventories.Queries.GetInventoryById;

namespace WebAPI.Modules;

public class InventoriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("api/inventories").RequireAuthorization();

        groups.MapGet("", GetInventories);
        groups.MapGet("{id:guid}", GetInventoryById);
        groups.MapPost("", CreateInventory);
        groups.MapPut("{id:guid}", UpdateInventory);
        groups.MapDelete("{id:guid}", ToggleInventory);

    }

    private static async Task<IResult> GetInventories(ISender sender) => Results.Ok(await sender.Send(new GetInventoriesQuery()));

    private static async Task<IResult> GetInventoryById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetInventoryByIdQuery(id)));

    private static async Task<IResult> CreateInventory(ISender sender, CreateInventoryCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateInventory(ISender sender, Guid id, UpdateInventoryCommand command)
    {
        if (id != command.InventoryId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleInventory(ISender sender, Guid id)
    {
        await sender.Send(new ToggleInventoryCommand(id));

        return Results.NoContent();
    }
}
