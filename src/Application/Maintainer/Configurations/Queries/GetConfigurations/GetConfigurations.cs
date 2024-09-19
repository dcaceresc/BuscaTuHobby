namespace Application.Maintainer.Configurations.Queries.GetConfigurations;
public record GetConfigurations : IRequest<ApiResponse<List<ConfigurationDto>>>;

public class GetConfiguratiosnHandler(IApplicationDbContext context, IApiResponseService responseService, IMapper mapper) : IRequestHandler<GetConfigurations, ApiResponse<List<ConfigurationDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IMapper _mapper = mapper;

    public async Task<ApiResponse<List<ConfigurationDto>>> Handle(GetConfigurations request, CancellationToken cancellationToken)
    {
        try
        {
            var configurations = await _context.Configurations
                .ProjectTo<ConfigurationDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(configurations);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<ConfigurationDto>>("Ah ocurrido un problema al obtener las configuraciones");
        }
    }
}
