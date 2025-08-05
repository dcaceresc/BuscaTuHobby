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
        var group = app.MapGroup("api/security/roles").RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));

        group.MapGet("", GetRoles);
        group.MapGet("{id:guid}", GetRoleById);
        group.MapPost("", CreateUser);
        group.MapPut("{id:guid}", UpdateRole);
        group.MapDelete("{id:guid}", ToggleRole);

    }


    private static async Task<IResult> GetRoles(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetRoles()));

    private static async Task<IResult> GetRoleById(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new GetRoleById(id)));

    private static async Task<IResult> CreateUser(IRequestDispatcher sender, CreateRole command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateRole(IRequestDispatcher sender, Guid id, UpdateRole command)
    {
        if (id != command.RoleId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la del permiso {command.RoleId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleRole(IRequestDispatcher sender, Guid id) => Results.Ok(await sender.Send(new ToggleRole(id)));


}
