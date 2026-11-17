using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Infrastructure.Services;

/// <summary>
/// Resuelve la tienda desde el host de la URL y delega en el conector que esa fila declara. Añadir una segunda
/// tienda Shopify es añadir una fila con la misma clave, no una rama más de código.
/// </summary>
public sealed class StoreProductLoader(IStoreContextProvider provider, IEnumerable<IStoreConnector> connectors)
    : IStoreProductLoader
{
    private const int MaximumUrlLength = 8192;
    private readonly IStoreContextProvider _provider = provider;
    private readonly IReadOnlyList<IStoreConnector> _connectors = connectors.ToArray();

    public async Task<IrionProductPreviewDto> LoadAsync(string url, CancellationToken cancellationToken)
    {
        if (!IsProductUrl(url, out var host)) throw new StoreOfferUrlException();

        var store = await _provider.FindByHostAsync(host, cancellationToken)
            ?? throw new StoreOfferUrlException();
        var connector = _connectors.FirstOrDefault(candidate =>
                string.Equals(candidate.Key, store.ConnectorKey, StringComparison.OrdinalIgnoreCase))
            ?? throw new StoreConfigurationException(
                $"La tienda {store.StoreName} declara un conector desconocido: {store.ConnectorKey}.");

        return await connector.LoadProductAsync(store, url, cancellationToken);
    }

    private static bool IsProductUrl(string url, out string host)
    {
        host = string.Empty;
        if (string.IsNullOrWhiteSpace(url) || url.Length > MaximumUrlLength ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || !uri.IsDefaultPort || uri.UserInfo.Length != 0)
            return false;

        host = StoreContext.NormalizeHost(uri.Host);
        return host.Length > 0;
    }
}
