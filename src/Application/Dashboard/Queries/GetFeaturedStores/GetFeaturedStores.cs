namespace Application.Dashboard.Queries.GetFeaturedStores;

public record GetFeaturedStores() : IRequest<ApiResponse<List<FeaturedStoreDto>>>;

public class GetFeaturedStoresHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetFeaturedStores, ApiResponse<List<FeaturedStoreDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<FeaturedStoreDto>>> Handle(GetFeaturedStores request, CancellationToken cancellationToken)
    {
        try
        {
            var stores = await _context.Stores
                .Where(s => s.IsActive)
                .OrderBy(s => s.StoreOrder)
                .Select(s => new FeaturedStoreDto
                {
                    StoreId = s.StoreId,
                    StoreName = s.StoreName,
                    StoreIcon = s.StoreIcon,
                    StoreWebSite = s.StoreWebSite,
                    ProductCount = s.Inventories.Count(i => i.IsActive && i.Product.IsActive),
                    OfferCount = s.Inventories.Count(i => i.IsActive && i.Product.IsActive && i.DiscountPercentage > 0)
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(stores);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<FeaturedStoreDto>>("Ha ocurrido un error al obtener las tiendas destacadas.");
        }
    }
}
