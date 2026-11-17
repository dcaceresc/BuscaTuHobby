using Application.Common.Interfaces;

namespace Application.Common.Exceptions;

// La importación manual de Irion sigue siendo la misma ruta, así que su texto se conserva palabra por palabra
// cuando la tienda no se puede resolver. Con una tienda resuelta, el mensaje se construye con esa tienda y con
// su host, de modo que para Irion el texto resultante es idéntico al histórico.
public sealed class StoreOfferUrlException(string? message = null) : Exception(message ?? DefaultMessage)
{
    public const string DefaultMessage =
        "Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto";

    public static StoreOfferUrlException ForStore(StoreContext store) => new(
        $"Introduce una URL válida de {store.StoreName}: https://{store.Host}/products/nombre-del-producto o https://{store.Host}/collections/coleccion/products/nombre-del-producto");
}

// El nombre de la tienda entra en el mensaje porque ahora puede ser cualquiera; el resto del texto es el
// contrato que ya devolvía el endpoint de Irion.
public sealed class StoreRateLimitException(string storeName, string? retryGuidance = null) : Exception(
    $"{storeName} está limitando las consultas (HTTP 429). " + (retryGuidance ?? "Inténtalo más tarde."));

// Resultado explícito del portal de robots: la ruta no se descarta en silencio ni se consulta igualmente.
public sealed class StorePathDisallowedException(string storeName, string host, string path) : Exception(
    $"robots.txt de {host} excluye {path}: no se consultó la ficha de {storeName}.");

// Configuración ilegible o incoherente. Falla de forma explícita en vez de asumir moneda o presupuesto.
public sealed class StoreConfigurationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
