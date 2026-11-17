namespace Application.Common.Ingestion;

// Decisión de una vuelta programada para una tienda: se ejecuta o se omite por un motivo escrito. Guardar el motivo
// en vez de un booleano es lo que permite explicar después por qué una tienda no se actualizó.
public sealed record IngestionScheduleEntry(StoreContext Store, string? SkipReason)
{
    public bool IsDue => SkipReason is null;
}

/// <summary>
/// Qué tiendas entran en una vuelta programada. No toca la red ni la base: recibe las tiendas que la configuración ya
/// resolvió y las ejecuciones abiertas, y decide quién se ejecuta. Aislado para poder comprobarlo sin infraestructura.
/// </summary>
public static class IngestionSchedule
{
    public static IReadOnlyList<IngestionScheduleEntry> Plan(
        IReadOnlyList<StoreContext> stores, IReadOnlySet<Guid> runningStoreIds)
    {
        ArgumentNullException.ThrowIfNull(stores);
        ArgumentNullException.ThrowIfNull(runningStoreIds);
        return stores.Select(store => new IngestionScheduleEntry(store, SkipReason(store, runningStoreIds))).ToArray();
    }

    public static string RunInProgressReason(StoreContext store) =>
        $"La tienda {store.StoreName} ya tiene una ingesta en curso: la vuelta la omite.";

    public static string NoConnectorReason(StoreContext store) =>
        $"La tienda {store.StoreName} no tiene conector configurado: no participa de la ingesta programada.";

    private static string? SkipReason(StoreContext store, IReadOnlySet<Guid> runningStoreIds) =>
        string.IsNullOrWhiteSpace(store.ConnectorKey) ? NoConnectorReason(store)
        : runningStoreIds.Contains(store.StoreId) ? RunInProgressReason(store)
        : null;
}
