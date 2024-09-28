namespace Application.Maintainer.Communes.Queries.GetCommunesByRegionId;
public record GetCommunesByRegionId(Guid RegionId) : IRequest<ApiResponse<List<CommuneByRegion>>>;

public class GetCommunesByRegionIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCommunesByRegionId, ApiResponse<List<CommuneByRegion>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CommuneByRegion>>> Handle(GetCommunesByRegionId request, CancellationToken cancellationToken)
    {
        try
        {
            var communes = await _context.Communes
                .Where(x => x.RegionId == request.RegionId && x.IsActive)
                .AsNoTracking()
                .ProjectTo<CommuneByRegion>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(communes);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<CommuneByRegion>>("Ocurrió un error al obtener las comunas");
        }
    }
}
