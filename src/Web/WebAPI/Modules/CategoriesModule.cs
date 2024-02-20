using Application.Maintainer.Categories.Commands.CreateCategory;
using Application.Maintainer.Categories.Commands.ToggleCategory;
using Application.Maintainer.Categories.Commands.UpdateCategory;
using Application.Maintainer.Categories.Queries.GetCategories;
using Application.Maintainer.Categories.Queries.GetCategoryById;

namespace WebAPI.Modules;

public class CategoriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("api/categories").RequireAuthorization();

        group.MapGet("", GetCategories);
        group.MapGet("{id:guid}", GetCategoryById);
        group.MapPost("", CreateCategory);
        group.MapPut("{id:guid}", UpdateCategory);
        group.MapDelete("{id:guid}", ToggleCategory);

    }

    private static async Task<IResult> GetCategories(ISender sender) => Results.Ok(await sender.Send(new GetCategoriesQuery()));

    private static async Task<IResult> GetCategoryById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetCategoryByIdQuery(id)));

    private static async Task<IResult> CreateCategory(ISender sender, CreateCategoryCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateCategory(ISender sender, Guid id, UpdateCategoryCommand command)
    {
        if (id != command.CategoryId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleCategory(ISender sender, Guid id)
    {
        await sender.Send(new ToggleCategoryCommand(id));

        return Results.NoContent();
    }
}
