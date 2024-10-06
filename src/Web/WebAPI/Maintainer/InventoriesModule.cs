
using Application.Maintainer.Inventories.Commands.CreateInventory;
using Application.Maintainer.Inventories.Commands.ToggleInventory;
using Application.Maintainer.Inventories.Commands.UpdateInventory;
using Application.Maintainer.Inventories.Queries.GetInventories;
using Application.Maintainer.Inventories.Queries.GetInventoryById;
using Domain.Common;

namespace WebAPI.Maintainer;

public class InventoriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("api/maintainer/inventories")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        groups.MapGet("", GetInventories);
        groups.MapGet("{id:guid}", GetInventoryById);
        groups.MapPost("", CreateInventory);
        groups.MapPut("{id:guid}", UpdateInventory);
        groups.MapDelete("{id:guid}", ToggleInventory);

    }

    private static async Task<IResult> GetInventories(ISender sender) => Results.Ok(await sender.Send(new GetInventories()));

    private static async Task<IResult> GetInventoryById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetInventoryById(id)));

    private static async Task<IResult> CreateInventory(ISender sender, CreateInventory command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateInventory(ISender sender, Guid id, UpdateInventory command)
    {
        if (id != command.InventoryId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id del inventario {command.InventoryId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleInventory(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleInventory(id)));
}
