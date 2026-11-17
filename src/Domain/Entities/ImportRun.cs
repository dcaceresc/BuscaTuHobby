namespace Domain.Entities;

// Estado de una ejecución de descubrimiento. Los números se guardan en la columna y son el contrato que leen las
// consultas de reanudación, así que un valor nuevo no puede reutilizar los existentes.
public enum ImportRunStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2
}

// Una ejecución de descubrimiento del catálogo de una tienda. Guarda con qué presupuesto de peticiones arrancó y
// qué encontró, de modo que una ingesta se pueda auditar y reanudar sin adivinar por dónde iba.
public class ImportRun
{
    private ImportRun(Guid storeId, int requestBudget, DateTime startedAtUtc)
    {
        ImportRunId = Guid.NewGuid();
        StoreId = storeId;
        RequestBudget = requestBudget;
        StartedAtUtc = startedAtUtc;
        Status = ImportRunStatus.Running;
    }

    public Guid ImportRunId { get; private set; }
    public Guid StoreId { get; private set; }
    public DateTime StartedAtUtc { get; private set; }

    // Nulo mientras la ejecución sigue abierta: el fin se escribe una sola vez, al cerrarla.
    public DateTime? FinishedAtUtc { get; private set; }
    public ImportRunStatus Status { get; private set; }

    // Tope de peticiones con el que la ejecución arrancó. Se copia del presupuesto de la tienda en vez de leerlo
    // después, porque la configuración de la tienda puede cambiar y la auditoría tiene que hablar del valor real.
    public int RequestBudget { get; private set; }

    public int DiscoveredCount { get; private set; }
    public int StoredCount { get; private set; }
    public int SkippedCount { get; private set; }
    public int FailedCount { get; private set; }

    public virtual Store Store { get; private set; } = default!;
    public virtual ICollection<ImportItem> Items { get; private set; } = default!;

    public static ImportRun Create(Guid storeId, int requestBudget, DateTime startedAtUtc)
    {
        if (storeId == Guid.Empty)
            throw new ArgumentException("La tienda de la ejecución es obligatoria.", nameof(storeId));
        if (requestBudget <= 0)
            throw new ArgumentException("El presupuesto de peticiones de la ejecución debe ser positivo.",
                nameof(requestBudget));
        if (startedAtUtc == default || startedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("El inicio de la ejecución debe ser una fecha UTC.", nameof(startedAtUtc));

        return new ImportRun(storeId, requestBudget, startedAtUtc);
    }

    // Los contadores se suman por lote porque el recorrido descarta varias entradas antes de la primera escritura.
    public void RecordDiscovered(int count)
    {
        EnsureOpen();
        ValidateCount(count);
        DiscoveredCount += count;
    }

    public void RecordStored(int count)
    {
        EnsureOpen();
        ValidateCount(count);
        StoredCount += count;
    }

    public void RecordSkipped(int count)
    {
        EnsureOpen();
        ValidateCount(count);
        SkippedCount += count;
    }

    public void RecordFailed(int count)
    {
        EnsureOpen();
        ValidateCount(count);
        FailedCount += count;
    }

    public void Complete(DateTime finishedAtUtc)
    {
        EnsureOpen();
        ValidateFinish(finishedAtUtc);
        FinishedAtUtc = finishedAtUtc;
        Status = ImportRunStatus.Completed;
    }

    public void Fail(DateTime finishedAtUtc)
    {
        EnsureOpen();
        ValidateFinish(finishedAtUtc);
        FinishedAtUtc = finishedAtUtc;
        Status = ImportRunStatus.Failed;
    }

    // Reanudar reabre una ejecución interrumpida o fallida. Una ejecución completada no se reanuda: su catálogo ya
    // se guardó y refrescarlo es una ejecución nueva, que es la que decide qué URL quedó desactualizada.
    public void Resume()
    {
        if (Status == ImportRunStatus.Completed)
            throw new InvalidOperationException("Una ejecución completada no se reanuda: crea una ejecución nueva.");
        Status = ImportRunStatus.Running;
        FinishedAtUtc = null;
    }

    private void EnsureOpen()
    {
        if (Status != ImportRunStatus.Running)
            throw new InvalidOperationException("La ejecución ya está cerrada.");
    }

    private void ValidateFinish(DateTime finishedAtUtc)
    {
        if (finishedAtUtc == default || finishedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("El fin de la ejecución debe ser una fecha UTC.", nameof(finishedAtUtc));
        if (finishedAtUtc < StartedAtUtc)
            throw new ArgumentException("El fin de la ejecución no puede ser anterior al inicio.",
                nameof(finishedAtUtc));
    }

    private static void ValidateCount(int count)
    {
        if (count < 0)
            throw new ArgumentException("El contador de la ejecución no puede ser negativo.", nameof(count));
    }
}
