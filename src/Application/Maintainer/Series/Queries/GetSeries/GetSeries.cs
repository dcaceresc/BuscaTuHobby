namespace Application.Maintainer.Series.Queries.GetSeries;

public record GetSeries : IRequest<ApiResponse<List<SerieDto>>>;

public class GetSeriesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetSeries, ApiResponse<List<SerieDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<SerieDto>>> Handle(GetSeries request, CancellationToken cancellationToken)
    {
        try
        {
            var series = await _context.Series
            .AsNoTracking()
            .ProjectTo<SerieDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(series);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<SerieDto>>("Ocurrió un error al obtener las series");
        }
    }
}