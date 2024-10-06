﻿using Application.Maintainer.Categories.Commands.CreateCategory;
using Application.Maintainer.Categories.Commands.ToggleCategory;
using Application.Maintainer.Categories.Commands.UpdateCategory;
using Application.Maintainer.Categories.Queries.GetCategories;
using Application.Maintainer.Categories.Queries.GetCategoryById;
using Domain.Common;

namespace WebAPI.Maintainer;

public class CategoriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("api/maintainer/categories")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetCategories);
        group.MapGet("{id:guid}", GetCategoryById);
        group.MapPost("", CreateCategory);
        group.MapPut("{id:guid}", UpdateCategory);
        group.MapDelete("{id:guid}", ToggleCategory);

    }

    private static async Task<IResult> GetCategories(ISender sender) => Results.Ok(await sender.Send(new GetCategories()));

    private static async Task<IResult> GetCategoryById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetCategoryById(id)));

    private static async Task<IResult> CreateCategory(ISender sender, CreateCategory command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateCategory(ISender sender, Guid id, UpdateCategory command)
    {
        if (id != command.CategoryId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la de la categoría {command.CategoryId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleCategory(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleCategory(id)));
}
