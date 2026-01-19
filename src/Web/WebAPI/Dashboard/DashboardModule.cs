using Application.Dashboard.Queries.GetMenuCategories;

namespace WebAPI.Dashboard;

public class DashboardModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard");
        
        group.MapGet("menu-categories", GetMenuCategories);

    }
    
    private static async Task<IResult> GetMenuCategories(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenuCategories()));
}
