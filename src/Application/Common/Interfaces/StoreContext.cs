namespace Application.Common.Interfaces;

/// <summary>
/// Identidad real de una tienda para los conectores. El host, la moneda y el presupuesto vienen de la fila de
/// la tienda, no de literales del código: es lo que permite que una segunda tienda Shopify sea una fila más.
/// </summary>
public sealed record StoreContext(
    Guid StoreId,
    string StoreName,
    string Host,
    string ConnectorKey,
    string Currency,
    int RequestsPerMinute,
    int RequestBudget)
{
    /// <summary>
    /// Normaliza el host que se compara en ambos lados (fila y URL). El prefijo <c>www.</c> no cambia de sitio
    /// y una tienda que se configura sin él debe seguir resolviendo sus propias URL con él.
    /// </summary>
    // Host con el que se pide de verdad, tal como lo declara la fila. La comparación usa Host (sin "www."), pero pedir
    // con el host normalizado provoca un 301 en las tiendas que publican con "www" y el transporte no sigue
    // redirecciones a propósito: por eso se pide con el canónico y se compara con el normalizado.
    public string RequestHost { get; init; } = Host;

    public static string NormalizeHost(string host)
    {
        var trimmed = (host ?? string.Empty).Trim().TrimEnd('.').ToLowerInvariant();
        return trimmed.StartsWith("www.", StringComparison.Ordinal) ? trimmed[4..] : trimmed;
    }
}
