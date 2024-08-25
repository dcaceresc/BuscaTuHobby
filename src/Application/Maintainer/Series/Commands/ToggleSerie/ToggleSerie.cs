namespace Application.Maintainer.Series.Commands.ToggleSerie;

public record ToggleSerie(Guid SerieId) : IRequest<ApiResponse>;
public class ToggleSerieHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleSerie, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleSerie request, CancellationToken cancellationToken)
    {
        try
        {
            var serie = await _context.Series.FindAsync([request.SerieId], cancellationToken);

            Guard.Against.NotFound(serie, $"No existe serie con Id {request.SerieId}");

            serie.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la serie se actualizó correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la serie");
        }


    }
}