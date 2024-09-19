namespace Application.Maintainer.Configurations.Commands.CreateConfiguration;
public record CreateConfiguration(string ConfigurationName, string ConfigurationValue) : IRequest<ApiResponse>;

public class CreateConfigurationHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateConfiguration, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateConfiguration request, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = Configuration.Create(request.ConfigurationName, request.ConfigurationValue);

            _context.Configurations.Add(configuration);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Configuración creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un problema al crear la configuración");
        }

    }
}
