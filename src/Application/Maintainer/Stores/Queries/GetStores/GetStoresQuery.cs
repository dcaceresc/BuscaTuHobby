namespace Application.Maintainer.Stores.Queries.GetStores;

public record GetStoresQuery : IRequest<ApiResponse<List<StoreDto>>>;

public class GetStoresQueryHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetStoresQuery, ApiResponse<List<StoreDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<StoreDto>>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .ProjectTo<StoreDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(stores);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<StoreDto>>("Ocurrió un error al obtener las tiendas");
        }
    }
}