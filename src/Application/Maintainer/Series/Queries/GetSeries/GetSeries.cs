namespace Application.Maintainer.Series.Queries.GetSeries;

public record GetSeries : IRequest<ApiResponse<List<SerieDto>>>;

public class GetSeriesHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetSeries, ApiResponse<List<SerieDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<SerieDto>>> Handle(GetSeries request, CancellationToken cancellationToken)
    {
        try
        {
            var series = await _context.Series
            .AsNoTracking()
            .Select(x => new SerieDto
            {
                SerieId = x.SerieId,
                SerieName = x.SerieName,
                FranchiseName = x.Franchise.FranchiseName,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

            return _responseService.Success(series);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<SerieDto>>("Ocurrió un error al obtener las series");
        }
    }
}