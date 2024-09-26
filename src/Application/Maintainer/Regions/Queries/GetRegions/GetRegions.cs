namespace Application.Maintainer.Regions.Queries.GetRegions;
public record GetRegions : IRequest<ApiResponse<List<RegionDto>>>;

public class GetRegionsHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetRegions, ApiResponse<List<RegionDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<RegionDto>>> Handle(GetRegions request, CancellationToken cancellationToken)
    {
        try
        {
            var regions = await _context.Regions
            .AsNoTracking()
            .ProjectTo<RegionDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.RegionOrder)
            .ToListAsync(cancellationToken);

            return _responseService.Success(regions);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<RegionDto>>("No se pudo obtener las regiones");
        }
    }
}
