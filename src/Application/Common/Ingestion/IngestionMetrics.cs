namespace Application.Common.Ingestion;

// Una oferta de la tienda tal como la ve la consulta de estado. Elegible significa lo mismo que en la ficha de
// producto: la oferta y su tienda activas, el producto activo y la disponibilidad distinta de false.
public sealed record IngestionOfferReading(Guid ProductId, bool IsEligible, DateTime? ObservedAtUtc);

// Números del estado de una tienda calculados sobre las filas que la consulta ya trajo. Sin base de datos y sin reloj
// propio: "ahora" entra como parámetro para que el resultado sea reproducible.
public sealed record IngestionStoreMetrics(
    IngestionRunSummary? LastRun,
    int TotalProducts,
    int CoveredProducts,
    int OffersObservedLast24Hours,
    int OffersOlderThanSevenDays)
{
    public int CoveragePercentage => TotalProducts == 0
        ? 0
        : (int)Math.Round(CoveredProducts * 100m / TotalProducts, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Cobertura y frescura de una tienda. La cobertura habla del escaparate, así que sólo cuenta ofertas elegibles; la
/// frescura habla de la ingesta, así que cuenta toda oferta observada, incluida la que hoy no se publica. Una oferta
/// sin fecha de observación no es ni fresca ni vieja: no se sabe cuándo se vio por última vez.
/// </summary>
public static class IngestionMetrics
{
    public const int FreshHours = 24;
    public const int StaleDays = 7;

    public static IngestionStoreMetrics Compute(string storeName, IReadOnlyList<ImportRun> runs,
        IReadOnlyList<IngestionOfferReading> offers, int totalProducts, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(runs);
        ArgumentNullException.ThrowIfNull(offers);
        if (totalProducts < 0)
            throw new System.ArgumentException("El total de productos no puede ser negativo.", nameof(totalProducts));
        if (nowUtc == default || nowUtc.Kind != DateTimeKind.Utc)
            throw new System.ArgumentException("La fecha de referencia del estado debe ser una fecha UTC.",
                nameof(nowUtc));

        // La última ejecución es la que arrancó más tarde; el identificador desempata dos arranques del mismo instante.
        var last = runs
            .OrderByDescending(run => run.StartedAtUtc)
            .ThenByDescending(run => run.ImportRunId)
            .FirstOrDefault();
        var covered = offers.Where(offer => offer.IsEligible).Select(offer => offer.ProductId).Distinct().Count();
        var fresh = offers.Count(offer => offer.ObservedAtUtc >= nowUtc.AddHours(-FreshHours));
        var stale = offers.Count(offer => offer.ObservedAtUtc < nowUtc.AddDays(-StaleDays));

        return new IngestionStoreMetrics(last is null ? null : IngestionRunSummary.From(last, storeName),
            totalProducts, covered, fresh, stale);
    }
}
