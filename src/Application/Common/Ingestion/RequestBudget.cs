namespace Application.Common.Ingestion;

/// <summary>
/// Presupuesto de peticiones de una ejecución. Cada documento del sitemap gasta una petición al descargarse y
/// cada URL encolada reserva otra, porque leerla cuesta una petición más: así el tope vale para la ejecución
/// completa y no sólo para el recorrido.
/// </summary>
public sealed class RequestBudget
{
    private int _remaining;

    public RequestBudget(int total)
    {
        if (total <= 0)
            throw new System.ArgumentException("El presupuesto de peticiones debe ser positivo.", nameof(total));
        Total = total;
        _remaining = total;
    }

    public int Total { get; }

    public int Remaining => _remaining;

    public int Consumed => Total - _remaining;

    // Devuelve falso en vez de lanzar: quedarse sin presupuesto es un final esperado del recorrido, no un error.
    public bool TryConsume()
    {
        if (_remaining == 0) return false;
        _remaining--;
        return true;
    }
}
