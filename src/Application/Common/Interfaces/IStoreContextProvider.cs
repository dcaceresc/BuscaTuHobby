namespace Application.Common.Interfaces;

/// <summary>
/// Traduce una fila de tienda en el contexto que consumen los conectores. Es la única lectura de configuración
/// de ingesta: si el host no corresponde a ninguna tienda activa con conector, no hay contexto y no se consulta.
/// </summary>
public interface IStoreContextProvider
{
    Task<StoreContext?> FindByHostAsync(string host, CancellationToken cancellationToken);
}
