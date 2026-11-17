using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Lee la configuración de ingesta de la base. Solo las tiendas activas y con conector declarado pueden
/// resolver un host: una tienda sin clave no participa y no se consulta su sitio.
/// </summary>
public sealed class StoreContextProvider(IApplicationDbContext context) : IStoreContextProvider
{
    private readonly IApplicationDbContext _context = context;

    public async Task<StoreContext?> FindByHostAsync(string host, CancellationToken cancellationToken)
    {
        var normalized = StoreContext.NormalizeHost(host);
        if (normalized.Length == 0) return null;

        // La tabla de tiendas es pequeña y su sitio web es texto libre, así que la comparación de host se hace
        // en memoria después de filtrar por las columnas que sí puede traducir el proveedor.
        var stores = await _context.Stores.AsNoTracking()
            .Where(store => store.ConnectorKey != null && store.IsActive)
            .OrderBy(store => store.StoreOrder)
            .ToListAsync(cancellationToken);

        foreach (var store in stores)
        {
            if (!Uri.TryCreate(store.StoreWebSite, UriKind.Absolute, out var site)) continue;
            if (!string.Equals(StoreContext.NormalizeHost(site.Host), normalized, StringComparison.Ordinal)) continue;

            var settings = StoreConnectorSettings.Parse(store.ConnectorSettings, store.StoreName);
            return new StoreContext(store.StoreId, store.StoreName, normalized, store.ConnectorKey!,
                settings.Currency, settings.RequestsPerMinute, settings.RequestBudget);
        }

        return null;
    }
}
