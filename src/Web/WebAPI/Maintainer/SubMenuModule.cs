using Application.Maintainer.SubMenus.Commands.CreateSubMenu;
using Application.Maintainer.SubMenus.Commands.ToggleCategory;
using Application.Maintainer.SubMenus.Commands.UpdateCategory;
using Application.Maintainer.SubMenus.Queries.GetSubMenuById;
using Application.Maintainer.SubMenus.Queries.GetSubMenus;
using Domain.Common;

namespace WebAPI.Maintainer;

public class SubMenuModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/submenus")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetSubMenus);
        group.MapGet("{id:guid}", GetSubMenuById);
        group.MapPost("", CreateSubMenu);
        group.MapPut("{id:guid}", UpdateSubMenu);
        group.MapDelete("{id:guid}", ToggleSubMenu);

    }

    private static async Task<IResult> GetSubMenus(ISender sender) => Results.Ok(await sender.Send(new GetSubMenus()));

    private static async Task<IResult> GetSubMenuById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetSubMenuById(id)));

    private static async Task<IResult> CreateSubMenu(ISender sender, CreateSubMenu command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateSubMenu(ISender sender, Guid id, UpdateSubMenu command)
    {
        if (id != command.SubMenuId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la escala {command.SubMenuId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleSubMenu(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleSubMenu(id)));
}
