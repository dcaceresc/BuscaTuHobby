namespace Application.Dashboard.Queries.GetMenuStores;

public record GetMenuStores() : IRequest<ApiResponse<List<MenuStoreDto>>>;

public class GetMenuStoresHandler(IApplicationDbContext context, IApiResponseService responseService) 
    : IRequestHandler<GetMenuStores, ApiResponse<List<MenuStoreDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<MenuStoreDto>>> Handle(GetMenuStores request, CancellationToken cancellationToken)
    {
        try
        {
            var stores = await _context.Stores
                .Where(s => s.IsActive)
                .OrderBy(s => s.StoreOrder)
                .Select(s => new MenuStoreDto
                {
                    StoreName = s.StoreName,
                    StoreIcon = s.StoreIcon,
                    StoreSlug = s.StoreSlug
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(stores);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<MenuStoreDto>>("Ah ocurrido un error al obtener las tiendas del menú.");
        }
    }
}

