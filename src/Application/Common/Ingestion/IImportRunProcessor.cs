namespace Application.Common.Ingestion;

// Resultado de una pasada sobre los elementos de una ejecución. Remaining son los que quedaron sin intentar porque
// el presupuesto o un límite de la tienda cortaron la pasada; Completed dice si la ejecución se cerró.
public sealed record ImportRunProcessingResult(
    int Stored,
    int Parked,
    int Skipped,
    int Failed,
    int Remaining,
    bool Completed,
    bool BudgetExhausted);

/// <summary>
/// Convierte lo que una ejecución encoló en ofertas y productos reales. Es la superficie que usará el disparo manual
/// del backoffice y la automatización: hoy no expone ninguna ruta nueva.
/// </summary>
public interface IImportRunProcessor
{
    /// <summary>
    /// Clasifica los productos que ya están en el catálogo sin volver a pedir fichas: la ingesta idempotente no
    /// reprocesa una URL ya guardada, así que sin esta pasada esos productos se quedarían sin categoría para siempre.
    /// Devuelve cuántos productos se clasificaron.
    /// </summary>
    Task<int> ClassifyExistingAsync(CancellationToken cancellationToken);

    Task<ImportRunProcessingResult> ProcessAsync(
        StoreContext store, Guid importRunId, DateTime observedAtUtc, CancellationToken cancellationToken);
}
