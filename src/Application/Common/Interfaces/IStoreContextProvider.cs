namespace Application.Common.Interfaces;

/// <summary>
/// Traduce una fila de tienda en el contexto que consumen los conectores. Es la única lectura de configuración
/// de ingesta: si el host no corresponde a ninguna tienda activa con conector, no hay contexto y no se consulta.
/// Las tres lecturas comparten el mismo predicado, de modo que el disparo manual y la pasada programada no pueden
/// ver conjuntos de tiendas distintos.
/// </summary>
public interface IStoreContextProvider
{
    Task<StoreContext?> FindByHostAsync(string host, CancellationToken cancellationToken);

    /// <summary>
    /// Resuelve una tienda configurada por su slug. Es lo que usa el disparo manual: una tienda desconocida o sin
    /// conector no tiene contexto y se rechaza antes de pedir nada a la red.
    /// </summary>
    Task<StoreContext?> FindBySlugAsync(string storeSlug, CancellationToken cancellationToken);

    /// <summary>
    /// Todas las tiendas activas con conector declarado, en el orden configurado. Es la lista que recorre la pasada
    /// programada: una tienda sin conector o cerrada no participa de la ingesta automática.
    /// </summary>
    Task<IReadOnlyList<StoreContext>> ListActiveAsync(CancellationToken cancellationToken);
}
