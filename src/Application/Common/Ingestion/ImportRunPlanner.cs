namespace Application.Common.Ingestion;

// Arranque de una ejecución: la fila que la audita, los elementos que quedan por procesar y lo que el recorrido
// descartó. Se devuelve junto para que el llamador no tenga que volver a consultar lo que acaba de escribir.
public sealed record ImportRunPlan(
    ImportRun Run, IReadOnlyList<ImportItem> Items, CatalogDiscoveryResult Discovery);

/// <summary>
/// Decide qué se encola en una ejecución y qué se vuelve a procesar al reanudarla. No toca la base ni la red: es la
/// regla de idempotencia y reanudación, aislada para que se pueda comprobar sin infraestructura.
/// </summary>
public static class ImportRunPlanner
{
    public const string ForeignItemsMessage = "Los elementos no pertenecen a la ejecución.";

    /// <summary>
    /// Crea la ejecución y sus elementos. Una URL que una ejecución completada anterior ya guardó no se vuelve a
    /// encolar: se registra como descartada con su motivo, para que el contador lo refleje.
    /// </summary>
    public static ImportRunPlan Start(StoreContext store, RequestBudget budget,
        CatalogDiscoveryResult discovery, IReadOnlySet<string> alreadyStored, DateTime startedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(discovery);
        ArgumentNullException.ThrowIfNull(alreadyStored);

        // El presupuesto queda escrito en la fila: sin él no se puede auditar si la ejecución respetó su tope.
        var run = ImportRun.Create(store.StoreId, budget.Total, startedAtUtc);
        var items = new List<ImportItem>();
        var stored = new HashSet<string>(alreadyStored, StringComparer.OrdinalIgnoreCase);

        // El índice único de la base ya impide repetir una URL en la ejecución. Se deduplica también aquí para que un
        // sitemap que repite una entrada no haga fallar la escritura completa.
        var queued = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skipped = discovery.CatalogSkipped.Count;
        var discovered = 0;
        foreach (var url in discovery.ProductUrls)
        {
            if (!queued.Add(url)) continue;
            if (stored.Contains(url))
            {
                items.Add(ImportItem.CreateSkipped(run.ImportRunId, url, CatalogSkip.AlreadyStoredMessage));
                skipped++;
                continue;
            }

            items.Add(ImportItem.Create(run.ImportRunId, url));
            discovered++;
        }

        // Lo descartado por el recorrido se audita como elemento cuando su referencia es una URL guardable; el resto
        // (documentos del sitemap) ya viaja en el resultado y sólo suma al contador.
        foreach (var discarded in discovery.ProductSkipped)
        {
            skipped++;
            if (!ImportItem.IsValidSourceUrl(discarded.Reference)) continue;
            items.Add(ImportItem.CreateSkipped(run.ImportRunId, discarded.Reference, discarded.Message));
        }

        // El contador describe lo que la ejecución encoló de verdad: la deduplicación y las URL que una ejecución
        // anterior ya guardó se resuelven aquí, así que el número del recorrido y la lista de elementos no pueden
        // separarse. Se cuenta lo escrito, no lo recibido.
        run.RecordDiscovered(discovered);
        run.RecordSkipped(skipped);
        return new ImportRunPlan(run, items, discovery);
    }

    /// <summary>
    /// Devuelve el trabajo de una reanudación: sólo los elementos pendientes o con un error transitorio, en orden
    /// de URL para que dos reanudaciones de la misma ejecución hagan lo mismo en el mismo orden.
    /// </summary>
    public static IReadOnlyList<ImportItem> Resume(ImportRun run, IReadOnlyList<ImportItem> items)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(items);
        if (items.Any(item => item.ImportRunId != run.ImportRunId))
            throw new System.ArgumentException(ForeignItemsMessage, nameof(items));

        var work = items
            .Where(item => item.IsProcessable)
            .OrderBy(item => item.SourceUrl, StringComparer.Ordinal)
            .ToArray();

        run.Resume();
        return work;
    }
}
