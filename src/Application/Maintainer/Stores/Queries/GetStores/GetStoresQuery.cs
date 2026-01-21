namespace Application.Maintainer.Stores.Queries.GetStores;

public record GetStoresQuery : IRequest<ApiResponse<List<StoreDto>>>;

public class GetStoresQueryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetStoresQuery, ApiResponse<List<StoreDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<StoreDto>>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Select(x => new StoreDto
                {
                    StoreId = x.StoreId,
                    StoreName = x.StoreName,
                    StoreWebSite = x.StoreWebSite,
                    StoreIcon = x.StoreIcon,
                    StoreOrder = x.StoreOrder,
                    StoreSlug = x.StoreSlug,
                    StoreAddress = x.StoreAddresses.Select(sa => $"{sa.Street} {sa.Commune.CommuneName}").ToList(),
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(stores);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<StoreDto>>("Ocurrió un error al obtener las tiendas");
        }
    }
}