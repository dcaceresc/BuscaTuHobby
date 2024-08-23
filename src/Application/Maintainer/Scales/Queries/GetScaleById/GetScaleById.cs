namespace Application.Maintainer.Scales.Queries.GetScaleById;

public record GetScaleById(Guid ScaleId) : IRequest<ApiResponse<ScaleVM>>;

public class GetScaleByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetScaleById, ApiResponse<ScaleVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ScaleVM>> Handle(GetScaleById request, CancellationToken cancellationToken)
    {
        try
        {
            var scale = await _context.Scales
              .AsNoTracking()
              .ProjectTo<ScaleVM>(_mapper.ConfigurationProvider)
              .FirstOrDefaultAsync(x => x.ScaleId == request.ScaleId, cancellationToken);

            Guard.Against.NotFound(scale, $"No existe una escala con la Id {request.ScaleId}");

            return _responseService.Success(scale);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<ScaleVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<ScaleVM>("Ocurrió un error al obtener la escala");
        }
    }
}