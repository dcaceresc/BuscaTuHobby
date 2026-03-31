using Application.Maintainer.Posts.Commands.CreatePost;
using Application.Maintainer.Posts.Commands.TogglePost;
using Application.Maintainer.Posts.Commands.UpdatePost;
using Application.Maintainer.Posts.Queries.GetPostById;
using Application.Maintainer.Posts.Queries.GetPosts;
using Domain.Common;

namespace WebAPI.Maintainer;

public class PostsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/posts")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetPosts);
        group.MapGet("{id:guid}", GetPostById);
        group.MapPost("", CreatePost);
        group.MapPut("{id:guid}", UpdatePost);
        group.MapDelete("{id:guid}", TogglePost);
    }

    private static async Task<IResult> GetPosts(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetPosts()));

    private static async Task<IResult> GetPostById(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new GetPostById(id)));

    private static async Task<IResult> CreatePost(IRequestDispatcher sender, CreatePost command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdatePost(IRequestDispatcher sender, Guid id, UpdatePost command)
    {
        if (id != command.PostId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id del post {command.PostId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> TogglePost(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new TogglePost(id)));
}
