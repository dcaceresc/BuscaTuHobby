using Application.Security.Roles.Commands.CreateRole;
using Application.Security.Roles.Commands.ToggleRole;
using Application.Security.Roles.Commands.UpdateRole;
using Application.Security.Roles.Queries.GetRoleById;
using Application.Security.Roles.Queries.GetRoles;
using Domain.Common;

namespace WebAPI.Security;

public class RoleModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/security/roles").RequireAuthorization("SuperAdmin");

        group.MapGet("", GetRoles);
        group.MapGet("{id:guid}", GetRoleById);
        group.MapPost("", CreateUser);
        group.MapPut("{id:guid}", UpdateUser);
        group.MapDelete("{id:guid}", ToggleUser);

    }


    private static async Task<IResult> GetRoles(ISender sender) => Results.Ok(await sender.Send(new GetRoles()));

    private static async Task<IResult> GetRoleById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetRoleById(id)));

    private static async Task<IResult> CreateUser(ISender sender, CreateRole command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateUser(ISender sender, Guid id, UpdateRole command)
    {
        if (id != command.RoleId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la del permiso {command.RoleId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleUser(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleRole(id)));


}
