namespace Application.Maintainer.Configurations.Commands.UpdateConfiguration;
public record UpdateConfiguration(Guid ConfigurationId, string ConfigurationName, string ConfigurationValue) : IRequest<ApiResponse>;

public class UpdateConfigurationHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateConfiguration, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateConfiguration request, CancellationToken cancellationToken)
    {
        try
        {
            var configuration = await _context.Configurations
                .Where(x => x.ConfigurationId == request.ConfigurationId)
                .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(configuration, $"No existe configuración con la Id {request.ConfigurationId}");

            configuration.Update(request.ConfigurationName, request.ConfigurationValue);

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
