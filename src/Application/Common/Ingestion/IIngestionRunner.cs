namespace Application.Common.Ingestion;

// Lo que una ejecución dejó escrito, con el nombre de la tienda que la pidió. Es el contrato que comparten el
// disparo manual y el estado: los dos hablan de la fila guardada, no de lo que la pasada creía estar haciendo.
public sealed record IngestionRunSummary(
    Guid ImportRunId,
    Guid StoreId,
    string StoreName,
    ImportRunStatus Status,
    int RequestBudget,
    DateTime StartedAtUtc,
    DateTime? FinishedAtUtc,
    int DiscoveredCount,
    int StoredCount,
    int SkippedCount,
    int FailedCount)
{
    public static IngestionRunSummary From(ImportRun run, string storeName)
    {
        ArgumentNullException.ThrowIfNull(run);
        return new IngestionRunSummary(run.ImportRunId, run.StoreId, storeName, run.Status, run.RequestBudget,
            run.StartedAtUtc, run.FinishedAtUtc, run.DiscoveredCount, run.StoredCount, run.SkippedCount,
            run.FailedCount);
    }
}

/// <summary>
/// El ejecutor de la ingesta: el mismo pipeline para el botón del backoffice y para la vuelta programada, de modo que
/// las dos puertas apliquen el mismo descubrimiento, el mismo presupuesto y la misma cortesía con la tienda.
/// </summary>
public interface IIngestionRunner
{
    /// <summary>
    /// Ingesta una tienda por su slug. Rechaza una tienda desconocida o sin conector antes de pedir nada, y una
    /// segunda ejecución mientras la primera siga abierta. El presupuesto opcional nunca supera el de la tienda.
    /// </summary>
    Task<IngestionRunSummary> RunAsync(string storeSlug, int? requestBudget, CancellationToken cancellationToken);

    /// <summary>
    /// Ejecuta la vuelta programada de todas las tiendas activas con conector, cada una con su presupuesto
    /// configurado. Omite la tienda con una ejecución en curso y devuelve sólo lo que se ejecutó de verdad.
    /// </summary>
    Task<IReadOnlyList<IngestionRunSummary>> RunScheduledAsync(CancellationToken cancellationToken);
}
