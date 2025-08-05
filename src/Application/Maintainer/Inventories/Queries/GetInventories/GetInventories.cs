namespace Application.Maintainer.Inventories.Queries.GetInventories;

public record GetInventories : IRequest<ApiResponse<List<InventoryDto>>>;

public class GetInventoriesHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetInventories, ApiResponse<List<InventoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<InventoryDto>>> Handle(GetInventories request, CancellationToken cancellationToken)
    {
        try
        {
            var inventories = await _context.Inventories
            .Include(x => x.Product)
            .Include(x => x.Store)
            .AsNoTracking()
            .Select(x => new InventoryDto
            {
                InventoryId = x.InventoryId,
                ProductName = x.Product.ProductName,
                StoreName = x.Store.StoreName,
                Price = x.Price.ToString("C0"),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

            return _responseService.Success(inventories);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<InventoryDto>>("No se pudo obtener los inventarios");
        }
    }
}