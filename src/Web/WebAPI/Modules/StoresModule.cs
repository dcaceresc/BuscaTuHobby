﻿using Application.Maintainer.Stores.Commands.CreateStore;
using Application.Maintainer.Stores.Commands.ToggleStore;
using Application.Maintainer.Stores.Commands.UpdateStore;
using Application.Maintainer.Stores.Queries.GetStoreById;
using Application.Maintainer.Stores.Queries.GetStores;

namespace WebAPI.Modules;

public class StoresModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/stores").RequireAuthorization();

        group.MapGet("", GetStores);
        group.MapGet("{id:guid}", GetStoreById);
        group.MapPost("", CreateStore);
        group.MapPut("{id:guid}", UpdateStore);
        group.MapDelete("{id:guid}", ToggleStore);

    }

    private static async Task<IResult> GetStores(ISender sender) => Results.Ok(await sender.Send(new GetStoresQuery()));

    private static async Task<IResult> GetStoreById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetStoreByIdQuery(id)));

    private static async Task<IResult> CreateStore(ISender sender, CreateStoreCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateStore(ISender sender, Guid id, UpdateStoreCommand command)
    {
        if (id != command.StoreId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleStore(ISender sender, Guid id)
    {
        await sender.Send(new ToggleStoreCommand(id));

        return Results.NoContent();
    }
}
