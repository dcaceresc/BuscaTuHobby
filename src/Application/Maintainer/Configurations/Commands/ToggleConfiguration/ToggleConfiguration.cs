namespace Application.Maintainer.Configurations.Commands.ToggleConfiguration;
public record ToggleConfiguration(Guid ConfigurationId) : IRequest<ApiResponse>;

public class ToggleConfigurationHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleConfiguration, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleConfiguration request, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await _context.Configurations
                .Where(x => x.ConfigurationId == request.ConfigurationId)
                .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(configuration, $"No existe configuración con la Id {request.ConfigurationId}");

            configuration.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Configuración actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un problema al actualizar la configuración");
        }
    }
}
