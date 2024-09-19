namespace Application.Maintainer.Configurations.Queries.GetConfigurationById;
public record GetConfigurationById(Guid ConfigurationId) : IRequest<ApiResponse<ConfigurationVM>>;

public class GetConfigurationByIdHandler(IApplicationDbContext context, IApiResponseService responseService, IMapper mapper) : IRequestHandler<GetConfigurationById, ApiResponse<ConfigurationVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IMapper _mapper = mapper;

    public async Task<ApiResponse<ConfigurationVM>> Handle(GetConfigurationById request, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await _context.Configurations
                .Where(x => x.ConfigurationId == request.ConfigurationId)
                .ProjectTo<ConfigurationVM>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(configuration, $"No existe configuración con la Id {request.ConfigurationId}");

            return _responseService.Success(configuration);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<ConfigurationVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<ConfigurationVM>("Ah ocurrido un problema al obtener la configuración");
        }
    }
}
