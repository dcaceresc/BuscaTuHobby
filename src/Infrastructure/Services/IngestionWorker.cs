using Application.Common.Ingestion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Dispara la ingesta sin que nadie la pida a mano: una vuelta cada <c>Ingestion:IntervalHours</c>. Está apagada por
/// configuración (el valor por defecto) y, apagada, no hace nada y lo dice en el registro. Un fallo dentro de una
/// vuelta queda registrado y nunca detiene el host.
/// </summary>
public sealed class IngestionWorker(
    IServiceScopeFactory scopeFactory,
    IngestionSettings settings,
    ILogger<IngestionWorker> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IngestionSettings _settings = settings;
    private readonly ILogger<IngestionWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("La ingesta programada está apagada ({Key}=false).", IngestionSettings.EnabledKey);
            return;
        }

        // La primera vuelta espera un intervalo completo: abrir el sitio no puede golpear las tiendas sin que nadie lo
        // haya pedido, y un reinicio en Development no debe convertirse en una ingesta por reinicio.
        _logger.LogInformation(
            "Ingesta programada activada: una vuelta cada {Hours} horas. La primera espera un intervalo completo.",
            _settings.IntervalHours);
        using var timer = new PeriodicTimer(_settings.Interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await RunOnceAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Apagar el host cancela la espera: es el final esperado, no un error.
        }
    }

    /// <summary>
    /// Una vuelta completa. Es pública para que una comprobación pueda ejercerla sin esperar el reloj del intervalo.
    /// </summary>
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return;

        try
        {
            // El ejecutor es de ámbito: cada vuelta nace con un contexto de base de datos propio y lo cierra al terminar.
            using var scope = _scopeFactory.CreateScope();
            var summaries = await scope.ServiceProvider.GetRequiredService<IIngestionRunner>()
                .RunScheduledAsync(cancellationToken);
            _logger.LogInformation("Ingesta programada: {Count} tienda(s) ejecutada(s).", summaries.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Una vuelta fallida no puede tumbar el host: se registra y la siguiente vuelta lo reintenta.
            _logger.LogError(exception,
                "La ingesta programada falló; el host sigue vivo y lo reintentará en la próxima vuelta.");
        }
    }
}
