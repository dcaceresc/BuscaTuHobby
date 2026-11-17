using Application.Common.Interfaces;
using Domain.Entities;
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
        foreach (var store in await ConfiguredStoresAsync(cancellationToken))
        {
            if (!string.Equals(HostOf(store), normalized, StringComparison.Ordinal)) continue;
            return ContextOf(store, normalized);
        }

        return null;
    }

    public async Task<StoreContext?> FindBySlugAsync(string storeSlug, CancellationToken cancellationToken)
    {
        var slug = (storeSlug ?? string.Empty).Trim();
        if (slug.Length == 0) return null;

        // El slug es único en la base, pero se compara en memoria por la misma razón que el host: el sitio web es
        // texto libre y el host hay que extraerlo. Sólo una fila configurada puede resolver una ingesta.
        foreach (var store in await ConfiguredStoresAsync(cancellationToken))
        {
            if (!string.Equals(store.StoreSlug, slug, StringComparison.OrdinalIgnoreCase)) continue;
            var host = HostOf(store);
            if (host.Length == 0) return null;
            return ContextOf(store, host);
        }

        return null;
    }

    public async Task<IReadOnlyList<StoreContext>> ListActiveAsync(CancellationToken cancellationToken)
    {
        var contexts = new List<StoreContext>();
        foreach (var store in await ConfiguredStoresAsync(cancellationToken))
        {
            // Una tienda cuyo sitio no es una URL utilizable no se puede ingerir: se omite en vez de detener la vuelta
            // entera. Unos ajustes ilegibles, en cambio, se propagan como error de configuración: son un dato que una
            // persona tiene que corregir, no algo que la automatización deba saltarse en silencio.
            var host = HostOf(store);
            if (host.Length == 0) continue;
            contexts.Add(ContextOf(store, host));
        }

        return contexts;
    }

    private Task<List<Store>> ConfiguredStoresAsync(CancellationToken cancellationToken) =>
        _context.Stores.AsNoTracking()
            .Where(store => store.ConnectorKey != null && store.IsActive)
            .OrderBy(store => store.StoreOrder)
            .ToListAsync(cancellationToken);

    private static string HostOf(Store store) =>
        Uri.TryCreate(store.StoreWebSite, UriKind.Absolute, out var site)
            ? StoreContext.NormalizeHost(site.Host)
            : string.Empty;

    private static StoreContext ContextOf(Store store, string host)
    {
        var settings = StoreConnectorSettings.Parse(store.ConnectorSettings, store.StoreName);
        return new StoreContext(store.StoreId, store.StoreName, host, store.ConnectorKey!, settings.Currency,
            settings.RequestsPerMinute, settings.RequestBudget)
        {
            // Se pide con el host tal como lo declara la fila: una tienda que publica con "www" responde 301 al host
            // normalizado y el transporte no sigue redirecciones a propósito, así que la ficha se perdería.
            RequestHost = Uri.TryCreate(store.StoreWebSite, UriKind.Absolute, out var site) ? site.Host : host
        };
    }
}
