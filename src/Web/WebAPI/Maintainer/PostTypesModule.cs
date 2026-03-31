using Application.Maintainer.PostTypes.Commands.CreatePostType;
using Application.Maintainer.PostTypes.Commands.TogglePostType;
using Application.Maintainer.PostTypes.Commands.UpdatePostType;
using Application.Maintainer.PostTypes.Queries.GetPostTypeById;
using Application.Maintainer.PostTypes.Queries.GetPostTypes;
using Domain.Common;

namespace WebAPI.Maintainer;

public class PostTypesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/post-types")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetPostTypes);
        group.MapGet("{id:guid}", GetPostTypeById);
        group.MapPost("", CreatePostType);
        group.MapPut("{id:guid}", UpdatePostType);
        group.MapDelete("{id:guid}", TogglePostType);
    }

    private static async Task<IResult> GetPostTypes(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetPostTypes()));

    private static async Task<IResult> GetPostTypeById(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new GetPostTypeById(id)));

    private static async Task<IResult> CreatePostType(IRequestDispatcher sender, CreatePostType command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdatePostType(IRequestDispatcher sender, Guid id, UpdatePostType command)
    {
        if (id != command.PostTypeId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id del tipo de post {command.PostTypeId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> TogglePostType(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new TogglePostType(id)));
}
