using Application.Dashboard.Commands.IncrementPostViewCount;
using Application.Dashboard.Commands.IncrementProductViewCount;
using Application.Dashboard.Queries.GetBestDeals;
using Application.Dashboard.Queries.GetMenuCategories;
using Application.Dashboard.Queries.GetMenuStores;
using Application.Dashboard.Queries.GetMostSearchedProducts;
using Application.Dashboard.Queries.GetPopularCategories;
using Application.Dashboard.Queries.GetRecentPosts;
using Application.Dashboard.Queries.GetFeaturedStores;
using Application.Dashboard.Queries.GetRecentActivity;
using Application.Dashboard.Queries.SearchProducts;

namespace WebAPI.Dashboard;

public class DashboardModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard");
        
        group.MapGet("menu-categories", GetMenuCategories);
        group.MapGet("menu-stores", GetMenuStores);
        group.MapGet("recent-posts", GetRecentPosts);
        group.MapPut("posts/{id:guid}/view", IncrementPostViewCount);
        group.MapGet("most-searched-products", GetMostSearchedProducts);
        group.MapPut("products/{id:guid}/view", IncrementProductViewCount);
        group.MapGet("best-deals", GetBestDeals);
        group.MapGet("popular-categories", GetPopularCategories);
        group.MapGet("featured-stores", GetFeaturedStores);
        group.MapGet("recent-activity", GetRecentActivity);
        group.MapGet("search-products", SearchProductsQuery);

    }
    
    private static async Task<IResult> GetMenuCategories(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenuCategories()));
    private static async Task<IResult> GetMenuStores(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMenuStores()));
    private static async Task<IResult> GetRecentPosts(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetRecentPosts()));
    private static async Task<IResult> IncrementPostViewCount(Guid id, IRequestDispatcher sender) => Results.Ok(await sender.Send(new IncrementPostViewCount(id)));
    private static async Task<IResult> GetMostSearchedProducts(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetMostSearchedProducts()));
    private static async Task<IResult> IncrementProductViewCount(Guid id, IRequestDispatcher sender) => Results.Ok(await sender.Send(new IncrementProductViewCount(id)));
    private static async Task<IResult> GetBestDeals(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetBestDeals()));
    private static async Task<IResult> GetPopularCategories(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetPopularCategories()));
    private static async Task<IResult> GetFeaturedStores(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetFeaturedStores()));
    private static async Task<IResult> GetRecentActivity(IRequestDispatcher sender) => Results.Ok(await sender.Send(new GetRecentActivity()));
    private static async Task<IResult> SearchProductsQuery(string? term, IRequestDispatcher sender) => Results.Ok(await sender.Send(new SearchProducts(term ?? string.Empty)));
}
