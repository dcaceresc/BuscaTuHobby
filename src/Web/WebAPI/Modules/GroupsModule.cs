using Application.Maintainer.Groups.Commands.CreateGroup;
using Application.Maintainer.Groups.Commands.ToggleGroup;
using Application.Maintainer.Groups.Commands.UpdateGroup;
using Application.Maintainer.Groups.Queries.GetGroupById;
using Application.Maintainer.Groups.Queries.GetGroups;

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

    private static async Task<IResult> GetGroups(ISender sender) => Results.Ok(await sender.Send(new GetGroupsQuery()));
    private static async Task<IResult> GetGroupById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetGroupByIdQuery(id)));

    private static async Task<IResult> CreateGroup(ISender sender, CreateGroupCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateGroup(ISender sender, Guid id, UpdateGroupCommand command)
    {
        if (id != command.GroupId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleGroup(ISender sender, Guid id)
    {
        await sender.Send(new ToggleGroupCommand(id));

        return Results.NoContent();
    }
}
