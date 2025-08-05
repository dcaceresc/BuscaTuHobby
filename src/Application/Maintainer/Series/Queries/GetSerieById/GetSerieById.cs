namespace Application.Maintainer.Series.Queries.GetSerieById;

public record GetSerieById(Guid SerieId) : IRequest<ApiResponse<SerieVM>>;
public class GetSerieByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetSerieById, ApiResponse<SerieVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<SerieVM>> Handle(GetSerieById request, CancellationToken cancellationToken)
    {
        try
        {
            var serie = await _context.Series
            .AsNoTracking()
            .Select(x => new SerieVM
            {
                SerieId = x.SerieId,
                FranchiseId = x.FranchiseId
            })
            .FirstOrDefaultAsync(x => x.SerieId == request.SerieId, cancellationToken);

            Guard.Against.NotFound(serie, $"No existe serie con la Id {request.SerieId}");

            return _responseService.Success(serie);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<SerieVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<SerieVM>("Ocurrió un error al obtener la serie");
        }
    }
}