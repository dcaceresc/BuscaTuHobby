using Application.Maintainer.Stores.Commands.CreateStore;
using Application.Maintainer.Stores.Commands.CreateStoreAddress;
using Application.Maintainer.Stores.Commands.DeleteStoreAddress;
using Application.Maintainer.Stores.Commands.ToggleStore;
using Application.Maintainer.Stores.Commands.UpdateStore;
using Application.Maintainer.Stores.Commands.UpdateStoreAddress;
using Application.Maintainer.Stores.Queries.GetStoreById;
using Application.Maintainer.Stores.Queries.GetStores;
using Domain.Common;

namespace WebAPI.Maintainer;

public class StoresModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/stores")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetStores);
        group.MapGet("{id:guid}", GetStoreById);
        group.MapPost("", CreateStore);
        group.MapPost("{id:guid}/address", CreateStoreAddress);
        group.MapPut("{id:guid}", UpdateStore);
        group.MapPut("{id:guid}/address/{storeAddressId:guid}", UpdateStoreAddress);
        group.MapDelete("{id:guid}", ToggleStore);
        group.MapDelete("{id:guid}/address/{storeAddressId:guid}", DeleteStoreAddress);

    }

    private static async Task<IResult> GetStores(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetStoresQuery()));

    private static async Task<IResult> GetStoreById(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new GetStoreByIdQuery(id)));

    private static async Task<IResult> CreateStore(IRequestDispatcher sender, CreateStore command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> CreateStoreAddress(IRequestDispatcher sender, Guid id, CreateStoreAddress command)
    {
        if (id != command.StoreId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la tienda {command.StoreId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> UpdateStore(IRequestDispatcher sender, Guid id, UpdateStore command)
    {
        if (id != command.StoreId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la tienda {command.StoreId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> UpdateStoreAddress(IRequestDispatcher sender, Guid id, Guid storeAddressId, UpdateStoreAddress command)
    {
        if (id != command.StoreId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la tienda {command.StoreId}" });

        if (storeAddressId != command.StoreAddressId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {storeAddressId} no coincide con la Id de la dirección de la tienda {command.StoreAddressId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleStore(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new ToggleStore(id)));

    private static async Task<IResult> DeleteStoreAddress(IRequestDispatcher sender, Guid id, Guid storeAddressId) => Results.Ok(await sender.Send(new DeleteStoreAddress(id, storeAddressId)));

}
