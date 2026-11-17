using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Domain.Entities;

namespace Infrastructure.Services;

// Arranque y reanudación de una ejecución contra la base real. La decisión de qué encolar y qué reprocesar vive en
// ImportRunPlanner; aquí sólo se leen y escriben las filas que esa decisión necesita.
public sealed class ImportRunService(IApplicationDbContext context, ICatalogDiscovery discovery) : IImportRunService
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICatalogDiscovery _discovery = discovery;

    public async Task<ImportRunPlan> StartAsync(
        StoreContext store, DateTime startedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);

        // El presupuesto sale de la fila de la tienda y se copia a la ejecución: nada más lo toca en esta pasada.
        var budget = new RequestBudget(store.RequestBudget);
        var discovered = await _discovery.DiscoverAsync(store, budget, cancellationToken);

        // Idempotencia: lo que una ejecución completada ya guardó no se vuelve a encolar. Se compara por texto y sin
        // distinguir mayúsculas porque es lo que hace el índice único de SQL Server con esta columna.
        var stored = await _context.ImportItems.AsNoTracking()
            .Where(item => item.ImportRun.StoreId == store.StoreId &&
                item.ImportRun.Status == ImportRunStatus.Completed &&
                item.State == ImportItemState.Stored)
            .Select(item => item.SourceUrl)
            .Distinct()
            .ToListAsync(cancellationToken);

        var plan = ImportRunPlanner.Start(store, budget, discovered,
            new HashSet<string>(stored, StringComparer.OrdinalIgnoreCase), startedAtUtc);

        _context.ImportRuns.Add(plan.Run);
        _context.ImportItems.AddRange(plan.Items);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task<IReadOnlyList<ImportItem>> ResumeAsync(Guid importRunId, CancellationToken cancellationToken)
    {
        var run = await _context.ImportRuns
            .FirstOrDefaultAsync(row => row.ImportRunId == importRunId, cancellationToken)
            ?? throw new NotFoundException($"La ejecución {importRunId} no existe.");

        var items = await _context.ImportItems
            .Where(item => item.ImportRunId == importRunId)
            .ToListAsync(cancellationToken);

        var work = ImportRunPlanner.Resume(run, items);
        await _context.SaveChangesAsync(cancellationToken);
        return work;
    }
}
