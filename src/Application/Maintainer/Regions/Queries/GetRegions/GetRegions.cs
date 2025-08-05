namespace Application.Maintainer.Regions.Queries.GetRegions;
public record GetRegions : IRequest<ApiResponse<List<RegionDto>>>;

public class GetRegionsHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetRegions, ApiResponse<List<RegionDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<RegionDto>>> Handle(GetRegions request, CancellationToken cancellationToken)
    {
        try
        {
            var regions = await _context.Regions
            .AsNoTracking()
            .Select(x => new RegionDto
            {
                RegionId = x.RegionId,
                RegionName = x.RegionName,
                RegionOrder = x.RegionOrder,
                IsActive = x.IsActive
            })
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
