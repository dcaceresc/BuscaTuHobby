using Application.Dashboard.Queries.GetMenuCategories;
using Application.Dashboard.Queries.GetMenuStores;

namespace WebAPI.Dashboard;

public class DashboardModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard");
        
        group.MapGet("menu-categories", GetMenuCategories);
        group.MapGet("menu-stores", GetMenuStores);

    }
    
    private static async Task<IResult> GetMenuCategories(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenuCategories()));
    private static async Task<IResult> GetMenuStores(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenuStores()));
}
