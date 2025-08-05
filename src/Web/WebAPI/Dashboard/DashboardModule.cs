using Application.Common.Mediator;
using Application.Dashboard.Queries.GetMenus;

namespace WebAPI.Dashboard;

public class DashboardModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard");

        group.MapGet("getmenu", GetMenus);
    }

    private static async Task<IResult> GetMenus(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenus()));
}
