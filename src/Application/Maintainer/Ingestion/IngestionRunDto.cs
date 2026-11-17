using Application.Common.Ingestion;

namespace Application.Maintainer.Ingestion;

// Contadores de una ejecución tal como los dejó escritos la ingesta. El disparo manual y el estado publican el mismo
// contrato: quien dispara ve lo que la fila guardó, no lo que la pasada creía estar haciendo.
public class IngestionRunDto
{
    public Guid ImportRunId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int Status { get; set; }
    public int RequestBudget { get; set; }
    public int DiscoveredCount { get; set; }
    public int StoredCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }

    public static IngestionRunDto From(IngestionRunSummary run) => new()
    {
        ImportRunId = run.ImportRunId,
        StartedAtUtc = run.StartedAtUtc,
        FinishedAtUtc = run.FinishedAtUtc,
        Status = (int)run.Status,
        RequestBudget = run.RequestBudget,
        DiscoveredCount = run.DiscoveredCount,
        StoredCount = run.StoredCount,
        SkippedCount = run.SkippedCount,
        FailedCount = run.FailedCount
    };
}
