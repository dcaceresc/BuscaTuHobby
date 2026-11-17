using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// Ejecuta una ingesta completa —descubrimiento y procesamiento— para una tienda o para todas las configuradas. La
/// resolución de la tienda, el presupuesto y el bloqueo de concurrencia viven aquí, en un solo sitio, para que el
/// botón del backoffice y la vuelta programada no puedan comportarse distinto.
/// </summary>
public sealed class IngestionRunner(
    IApplicationDbContext context,
    IStoreContextProvider stores,
    IImportRunService runs,
    IImportRunProcessor processor,
    ILogger<IngestionRunner> logger) : IIngestionRunner
{
    // Un presupuesto manual recorta el configurado y nunca lo amplía: el tope de cortesía lo aprobó el operador en la
    // fila de la tienda, así que el botón no puede saltárselo.
    private const int MinimumRequestBudget = 1;

    private readonly IApplicationDbContext _context = context;
    private readonly IStoreContextProvider _stores = stores;
    private readonly IImportRunService _runs = runs;
    private readonly IImportRunProcessor _processor = processor;
    private readonly ILogger<IngestionRunner> _logger = logger;

    public async Task<IngestionRunSummary> RunAsync(string storeSlug, int? requestBudget,
        CancellationToken cancellationToken)
    {
        var slug = (storeSlug ?? string.Empty).Trim();
        var store = await _stores.FindBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException($"La tienda {slug} no existe o no tiene ingesta configurada.");
        await EnsureNoOpenRunAsync(store, cancellationToken);

        var effective = requestBudget is { } budget
            ? store with { RequestBudget = Math.Clamp(budget, MinimumRequestBudget, store.RequestBudget) }
            : store;
        return await ExecuteAsync(effective, cancellationToken);
    }

    public async Task<IReadOnlyList<IngestionRunSummary>> RunScheduledAsync(CancellationToken cancellationToken)
    {
        var stores = await _stores.ListActiveAsync(cancellationToken);
        var open = (await _context.ImportRuns.AsNoTracking()
                .Where(run => run.Status == ImportRunStatus.Running)
                .Select(run => run.StoreId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var summaries = new List<IngestionRunSummary>();
        foreach (var entry in IngestionSchedule.Plan(stores, open))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!entry.IsDue)
            {
                // La omisión se registra con su motivo: una tienda que dejó de actualizarse tiene que ser explicable.
                _logger.LogInformation("Ingesta programada: {Store} omitida. {Reason}",
                    entry.Store.StoreName, entry.SkipReason);
                continue;
            }

            try
            {
                summaries.Add(await ExecuteAsync(entry.Store, cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // Una tienda rota no puede llevarse la vuelta entera: queda registrada y la próxima vuelta lo reintenta.
                _logger.LogError(exception,
                    "La ingesta programada de {Store} falló; se reintentará en la próxima vuelta.",
                    entry.Store.StoreName);
            }
        }

        return summaries;
    }

    // El bloqueo se pregunta por la ejecución abierta, no por el historial: una ejecución completada o fallida deja
    // de estorbar en cuanto se cierra.
    private async Task EnsureNoOpenRunAsync(StoreContext store, CancellationToken cancellationToken)
    {
        var alreadyRunning = await _context.ImportRuns.AsNoTracking()
            .AnyAsync(run => run.StoreId == store.StoreId && run.Status == ImportRunStatus.Running,
                cancellationToken);
        if (alreadyRunning)
            throw new IngestionConflictException(
                $"La tienda {store.StoreName} ya tiene una ingesta en curso: espera a que termine antes de lanzar otra.");
    }

    private async Task<IngestionRunSummary> ExecuteAsync(StoreContext store, CancellationToken cancellationToken)
    {
        // La misma fecha abre la ejecución y fecha la observación: la pasada entera habla de un solo instante, así que
        // el historial no puede quedar con dos "ahora" distintos dentro de la misma pasada.
        var startedAtUtc = DateTime.UtcNow;
        var plan = await _runs.StartAsync(store, startedAtUtc, cancellationToken);
        var result = await _processor.ProcessAsync(store, plan.Run.ImportRunId, startedAtUtc, cancellationToken);

        var summary = IngestionRunSummary.From(plan.Run, store.StoreName);
        _logger.LogInformation(
            "Ingesta de {Store}: ejecución {Run}, estado {Status}, descubiertas {Discovered}, guardadas {Stored}, descartadas {Skipped}, fallidas {Failed}, presupuesto {Budget}, aparcadas {Parked}, pendientes {Remaining}, completada {Completed}.",
            summary.StoreName, summary.ImportRunId, summary.Status, summary.DiscoveredCount, summary.StoredCount,
            summary.SkippedCount, summary.FailedCount, summary.RequestBudget, result.Parked, result.Remaining,
            result.Completed);
        return summary;
    }
}
