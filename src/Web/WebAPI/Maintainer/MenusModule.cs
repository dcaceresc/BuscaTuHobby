using Application.Maintainer.Menus.Commands.CreateMenu;
using Application.Maintainer.Menus.Commands.ToggleMenu;
using Application.Maintainer.Menus.Commands.UpdateMenu;
using Application.Maintainer.Menus.Queries.GetMenuById;
using Application.Maintainer.Menus.Queries.GetMenus;
using Domain.Common;

namespace WebAPI.Maintainer;

public class MenusModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("api/maintainer/menus")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        groups.MapGet("", GetMenus);
        groups.MapGet("{id:guid}", GetMenuById);
        groups.MapPost("", CreateMenu);
        groups.MapPut("{id:guid}", UpdateMenu);
        groups.MapDelete("{id:guid}", ToggleMenu);

    }

    private static async Task<IResult> GetMenus(ISender sender) => Results.Ok(await sender.Send(new GetMenus()));
    private static async Task<IResult> GetMenuById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetMenuById(id)));
    private static async Task<IResult> CreateMenu(ISender sender, CreateMenu command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateMenu(ISender sender, Guid id, UpdateMenu command)
    {
        if (id != command.MenuId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la del menu {command.MenuId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleMenu(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleMenu(id)));
}
