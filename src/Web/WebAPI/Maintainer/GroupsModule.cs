using Application.Maintainer.Groups.Commands.CreateGroup;
using Application.Maintainer.Groups.Commands.ToggleGroup;
using Application.Maintainer.Groups.Commands.UpdateGroup;
using Application.Maintainer.Groups.Queries.GetGroupById;
using Application.Maintainer.Groups.Queries.GetGroups;
using Domain.Common;

namespace WebAPI.Modules;

public class GroupsModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("api/groups").RequireAuthorization();

        groups.MapGet("", GetGroups);
        groups.MapGet("{id:guid}", GetGroupById);
        groups.MapPost("", CreateGroup);
        groups.MapPut("{id:guid}", UpdateGroup);
        groups.MapDelete("{id:guid}", ToggleGroup);

    }

    private static async Task<IResult> GetGroups(ISender sender) => Results.Ok(await sender.Send(new GetGroups()));
    private static async Task<IResult> GetGroupById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetGroupById(id)));
    private static async Task<IResult> CreateGroup(ISender sender, CreateGroup command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateGroup(ISender sender, Guid id, UpdateGroup command)
    {
        if (id != command.GroupId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la del grupo {command.GroupId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleGroup(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleGroup(id)));
}
