namespace Application.Dashboard.Queries.GetBestDeals;

public record GetBestDeals() : IRequest<ApiResponse<List<BestDealDto>>>;

public class GetBestDealsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetBestDeals, ApiResponse<List<BestDealDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<BestDealDto>>> Handle(GetBestDeals request, CancellationToken cancellationToken)
    {
        try
        {
            var deals = await _context.Inventories
                .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false &&
                    i.DiscountPercentage > 0)
                .OrderByDescending(i => i.DiscountPercentage)
                .Take(5)
                .Select(i => new BestDealDto
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    StoreName = i.Store.StoreName,
                    DiscountPercentage = i.DiscountPercentage,
                    ProductViewCount = i.Product.ProductViewCount
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(deals);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<BestDealDto>>("Ha ocurrido un error al obtener las mejores ofertas.");
        }
    }
}
