namespace Application.Maintainer.Inventories.Queries.GetInventories;

public record GetInventories : IRequest<ApiResponse<List<InventoryDto>>>;

public class GetInventoriesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetInventories, ApiResponse<List<InventoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<InventoryDto>>> Handle(GetInventories request, CancellationToken cancellationToken)
    {
        try
        {
            var inventories = await _context.Inventories
            .Include(x => x.Product)
            .Include(x => x.Store)
            .AsNoTracking()
            .ProjectTo<InventoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(inventories);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<InventoryDto>>("No se pudo obtener los inventarios");
        }
    }
}