namespace Application.Common.Ingestion;

/// <summary>
/// Arranque y reanudación de una ejecución de descubrimiento. Es la superficie que usará el disparo manual del
/// backoffice y la automatización posterior: hoy no expone ninguna ruta nueva.
/// </summary>
public interface IImportRunService
{
    Task<ImportRunPlan> StartAsync(StoreContext store, DateTime startedAtUtc, CancellationToken cancellationToken);

    Task<IReadOnlyList<ImportItem>> ResumeAsync(Guid importRunId, CancellationToken cancellationToken);
}
