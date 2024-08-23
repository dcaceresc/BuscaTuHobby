namespace Application.Maintainer.Scales.Queries.GetScales;

public record GetScales : IRequest<ApiResponse<List<ScaleDto>>>;

public class GetScalesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetScales, ApiResponse<List<ScaleDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<ScaleDto>>> Handle(GetScales request, CancellationToken cancellationToken)
    {
        try
        {
            var scales = await _context.Scales
                .AsNoTracking()
                .ProjectTo<ScaleDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(scales);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<ScaleDto>>("Ocurrió un error al obtener las escalas");
        }
    }
}

