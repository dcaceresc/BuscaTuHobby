using Application.Common.Ingestion;

namespace Application.Maintainer.Ingestion.Queries.GetIngestionStatus;

// Estado de una tienda configurada: su última ejecución, cuánto catálogo cubre y cómo de frescas están sus ofertas.
public class IngestionStatusDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreSlug { get; set; } = default!;
    public string? ConnectorKey { get; set; }
    public bool IsActive { get; set; }
    public int TotalProducts { get; set; }
    public int CoveredProducts { get; set; }
    public int CoveragePercentage { get; set; }
    public int OffersObservedLast24Hours { get; set; }
    public int OffersOlderThanSevenDays { get; set; }
    public IngestionRunDto? LastRun { get; set; }

    public static IngestionStatusDto From(IngestionStoreMetrics metrics) => new()
    {
        TotalProducts = metrics.TotalProducts,
        CoveredProducts = metrics.CoveredProducts,
        CoveragePercentage = metrics.CoveragePercentage,
        OffersObservedLast24Hours = metrics.OffersObservedLast24Hours,
        OffersOlderThanSevenDays = metrics.OffersOlderThanSevenDays,
        LastRun = metrics.LastRun is null ? null : IngestionRunDto.From(metrics.LastRun)
    };
}
