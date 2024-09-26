namespace Application.Maintainer.Communes.Commands.ToggleCommune;
public record ToggleCommune(Guid CommuneId) : IRequest<ApiResponse>;

public class ToggleCommuneHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleCommune, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleCommune request, CancellationToken cancellationToken)
    {
        try
        {
            var commune = await _context.Communes.FindAsync([request.CommuneId], cancellationToken);

            Guard.Against.NotFound(commune, $"No existe ciudad con la Id {request.CommuneId}");

            commune.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido actualizada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al actualizar la cuidad");
        }
    }
}
