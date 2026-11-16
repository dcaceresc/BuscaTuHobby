namespace Domain.Entities;

// Append-only observation of the price-relevant fields of an offer. It is never updated or deleted individually.
public class InventoryPriceHistory
{
    private InventoryPriceHistory(Guid inventoryId, int price, int originalPrice, int discountPercentage,
        bool? availability, DateTime observedAtUtc, bool isBaseline)
    {
        PriceHistoryId = Guid.NewGuid();
        InventoryId = inventoryId;
        Price = price;
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
        Availability = availability;
        ObservedAtUtc = observedAtUtc;
        IsBaseline = isBaseline;
    }

    public Guid PriceHistoryId { get; private set; }
    public Guid InventoryId { get; private set; }
    public int Price { get; private set; }
    public int OriginalPrice { get; private set; }
    public int DiscountPercentage { get; private set; }
    public bool? Availability { get; private set; }
    public DateTime ObservedAtUtc { get; private set; }
    public bool IsBaseline { get; private set; }

    public virtual Inventory Inventory { get; private set; } = default!;

    public static InventoryPriceHistory Create(Guid inventoryId, int price, int originalPrice, int discountPercentage,
        bool? availability, DateTime observedAtUtc, bool isBaseline = false)
    {
        ValidateReferences(inventoryId);
        return new InventoryPriceHistory(inventoryId, price, originalPrice, discountPercentage, availability,
            observedAtUtc, isBaseline);
    }

    private static void ValidateReferences(Guid inventoryId)
    {
        if (inventoryId == Guid.Empty)
            throw new ArgumentException("La oferta es obligatoria.");
    }
}
