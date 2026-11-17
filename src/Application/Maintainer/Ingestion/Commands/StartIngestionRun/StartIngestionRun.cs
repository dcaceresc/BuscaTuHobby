using Application.Common.Exceptions;
using Application.Common.Ingestion;

namespace Application.Maintainer.Ingestion.Commands.StartIngestionRun;

// Disparo manual de una ingesta. El slug identifica una tienda configurada y el presupuesto opcional sólo puede
// recortar el que la tienda ya tiene configurado.
public record StartIngestionRun(string StoreSlug, int? RequestBudget) : IRequest<ApiResponse<IngestionRunDto>>;

public class StartIngestionRunHandler(IIngestionRunner runner, IApiResponseService responseService)
    : IRequestHandler<StartIngestionRun, ApiResponse<IngestionRunDto>>
{
    private readonly IIngestionRunner _runner = runner;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<IngestionRunDto>> Handle(StartIngestionRun request, CancellationToken cancellationToken)
    {
        try
        {
            // El ejecutor hace el trabajo completo: descubrir el sitemap, procesar lo encolado y dejar la fila escrita.
            var summary = await _runner.RunAsync(request.StoreSlug, request.RequestBudget, cancellationToken);
            return _responseService.Success(IngestionRunDto.From(summary));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Application.Common.Exceptions.NotFoundException exception)
        {
            // Tienda desconocida o sin conector: se responde antes de haber pedido nada a la red.
            return _responseService.Fail<IngestionRunDto>(exception.Message);
        }
        catch (IngestionConflictException exception)
        {
            // Ya hay una ingesta abierta para esa tienda: la segunda no puede duplicar la fila ni el gasto.
            return _responseService.Fail<IngestionRunDto>(exception.Message);
        }
        catch (StoreConfigurationException exception)
        {
            // Una fila mal configurada se explica tal cual: es lo que el operador tiene que corregir.
            return _responseService.Fail<IngestionRunDto>(exception.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<IngestionRunDto>("No se pudo ejecutar la ingesta de la tienda.");
        }
    }
}
