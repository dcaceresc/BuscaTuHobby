namespace Application.Dashboard.Queries.GetRecentActivity;

public record GetRecentActivity() : IRequest<ApiResponse<List<RecentActivityDto>>>;

public class GetRecentActivityHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetRecentActivity, ApiResponse<List<RecentActivityDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<RecentActivityDto>>> Handle(GetRecentActivity request, CancellationToken cancellationToken)
    {
        try
        {
            var recentProducts = await _context.Products
                .OrderByDescending(p => p.Created)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    ActivityType = "Producto",
                    Title = "Nuevo producto agregado",
                    Description = p.ProductName + " agregado a la plataforma",
                    CreatedAt = p.Created
                })
                .ToListAsync(cancellationToken);

            var recentInventories = await _context.Inventories
                .OrderByDescending(i => i.Created)
                .Take(5)
                .Select(i => new RecentActivityDto
                {
                    ActivityType = "Oferta",
                    Title = i.DiscountPercentage > 0 ? "Nueva oferta disponible" : "Precio actualizado",
                    Description = i.Product.ProductName + " en " + i.Store.StoreName
                        + (i.DiscountPercentage > 0 ? " con " + i.DiscountPercentage + "% descuento" : ""),
                    CreatedAt = i.Created
                })
                .ToListAsync(cancellationToken);

            var recentStores = await _context.Stores
                .OrderByDescending(s => s.Created)
                .Take(5)
                .Select(s => new RecentActivityDto
                {
                    ActivityType = "Tienda",
                    Title = "Nueva tienda afiliada",
                    Description = s.StoreName + " se unió a la plataforma",
                    CreatedAt = s.Created
                })
                .ToListAsync(cancellationToken);

            var recentPosts = await _context.Posts
                .OrderByDescending(p => p.Created)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    ActivityType = "Publicación",
                    Title = "Nueva publicación",
                    Description = p.PostTitle,
                    CreatedAt = p.Created
                })
                .ToListAsync(cancellationToken);

            var result = recentProducts
                .Concat(recentInventories)
                .Concat(recentStores)
                .Concat(recentPosts)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();

            return _responseService.Success(result);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<RecentActivityDto>>("Ha ocurrido un error al obtener la actividad reciente.");
        }
    }
}
