namespace Application.Dashboard.Queries.GetPriceDrops;

public class PriceDropDto
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StoreName { get; set; } = default!;
    public string StoreSlug { get; set; } = default!;
    public string? SourceUrl { get; set; }
    public int CurrentPrice { get; set; }
    public int PreviousPrice { get; set; }
    public int DropPercentage { get; set; }
    public DateTime ObservedAtUtc { get; set; }
    public DateTime PreviousObservedAtUtc { get; set; }

    public static PriceDropDto From(PriceDrop drop) => new()
    {
        InventoryId = drop.Latest.InventoryId,
        ProductId = drop.Latest.ProductId,
        ProductName = drop.Latest.ProductName,
        StoreName = drop.Latest.StoreName,
        StoreSlug = drop.Latest.StoreSlug,
        SourceUrl = drop.Latest.SourceUrl,
        CurrentPrice = drop.Latest.Price,
        PreviousPrice = drop.PreviousPrice,
        DropPercentage = drop.DropPercentage,
        ObservedAtUtc = drop.Latest.ObservedAtUtc,
        PreviousObservedAtUtc = drop.PreviousObservedAtUtc
    };
}
