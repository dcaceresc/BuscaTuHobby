using Application.Dashboard.Queries.GetMenus;

namespace WebAPI.Dashboard;

public class DashboardModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dashboard");

        group.MapGet("GetMenus", GetMenus);
    }

    private static async Task<IResult> GetMenus(ISender sender) => Results.Ok(await sender.Send(new GetMenus()));
}
