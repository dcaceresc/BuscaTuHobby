namespace Application.Dashboard.Queries.GetPriceDrops;

// Un punto de la serie de precios junto a los datos que publica la lista. La selección compara la serie, así que el
// punto lleva sólo lo que la respuesta muestra y el identificador que desempata dos observaciones del mismo instante.
public sealed record PriceDropPoint(
    Guid InventoryId,
    Guid PriceHistoryId,
    Guid ProductId,
    string ProductName,
    string StoreName,
    string StoreSlug,
    string? SourceUrl,
    int Price,
    DateTime ObservedAtUtc);

// Una bajada: los dos puntos que la describen y el porcentaje que se lleva. El porcentaje se calcula una sola vez,
// con el mismo redondeo que el resto del catálogo, para que el umbral y la cifra publicada no se contradigan.
public sealed record PriceDrop(
    PriceDropPoint Latest,
    int PreviousPrice,
    DateTime PreviousObservedAtUtc,
    int DropPercentage);

/// <summary>
/// Selecciona las bajadas de una serie de precios. Una bajada es el último punto observado por debajo del anterior:
/// una oferta con un solo punto no se compara contra sí misma, un aumento no es una bajada y un precio anterior de
/// cero no produce porcentaje. No toca la base ni el reloj: recibe los puntos ya filtrados y devuelve la lista.
/// </summary>
public static class PriceDropSelection
{
    public const int MinimumPercent = 1;
    public const int MaximumPercent = 100;

    public static IReadOnlyList<PriceDrop> Select(IEnumerable<PriceDropPoint> points, int minPercent)
    {
        ArgumentNullException.ThrowIfNull(points);
        var threshold = Math.Clamp(minPercent, MinimumPercent, MaximumPercent);
        var drops = new List<PriceDrop>();

        foreach (var series in points.GroupBy(point => point.InventoryId))
        {
            // Los dos puntos más recientes de la oferta y nada más: el identificador desempata dos observaciones del
            // mismo instante para que el resultado no dependa del orden en que la consulta los devolvió.
            var ordered = series
                .OrderByDescending(point => point.ObservedAtUtc)
                .ThenByDescending(point => point.PriceHistoryId)
                .Take(2)
                .ToArray();
            if (ordered.Length < 2) continue;

            var latest = ordered[0];
            var previous = ordered[1];
            if (previous.Price <= 0 || latest.Price >= previous.Price) continue;

            // El umbral se compara antes de redondear: un 9.9% redondeado a 10 no puede colarse en un umbral de 10.
            var percentage = (previous.Price - latest.Price) * 100m / previous.Price;
            if (percentage < threshold) continue;
            drops.Add(new PriceDrop(latest, previous.Price, previous.ObservedAtUtc,
                (int)Math.Round(percentage, MidpointRounding.AwayFromZero)));
        }

        // Lo más barato primero y, entre dos ofertas al mismo precio, la observación más reciente: es el orden en que
        // la lista se puede leer como oportunidad.
        return drops
            .OrderBy(drop => drop.Latest.Price)
            .ThenByDescending(drop => drop.Latest.ObservedAtUtc)
            .ThenBy(drop => drop.Latest.InventoryId)
            .ToArray();
    }
}
