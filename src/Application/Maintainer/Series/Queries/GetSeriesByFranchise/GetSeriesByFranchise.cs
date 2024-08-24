namespace Application.Maintainer.Series.Queries.GetSeriesByFranchise;
public record GetSeriesByFranchise(Guid FranchiseId) : IRequest<ApiResponse<List<SerieByFranchiseDto>>>;

public class GetSeriesByFranchiseHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetSeriesByFranchise, ApiResponse<List<SerieByFranchiseDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<SerieByFranchiseDto>>> Handle(GetSeriesByFranchise request, CancellationToken cancellationToken)
    {
        try
        {
            var series = await _context.Series
            .Where(x => x.FranchiseId == request.FranchiseId)
            .AsNoTracking()
            .ProjectTo<SerieByFranchiseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(series);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<SerieByFranchiseDto>>("Ocurrió un error al obtener las series");
        }
    }
}
