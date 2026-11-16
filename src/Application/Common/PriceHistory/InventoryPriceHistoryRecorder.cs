namespace Application.Common.PriceHistory;

// The four fields that make up one observation of an offer's price.
public readonly record struct InventoryPricePoint(int Price, int OriginalPrice, int DiscountPercentage, bool? Availability)
{
    public static InventoryPricePoint Capture(Inventory inventory) =>
        new(inventory.Price, inventory.OriginalPrice, inventory.DiscountPercentage, inventory.Availability);
}

// The only place where the price series grows: a point is appended only when one of the four price-relevant
// fields differs from its pre-mutation value, so repeated identical observations never inflate the series.
public static class InventoryPriceHistoryRecorder
{
    public static void Record(IApplicationDbContext context, Inventory inventory, InventoryPricePoint before)
    {
        var current = InventoryPricePoint.Capture(inventory);
        if (current == before)
            return;

        context.InventoryPriceHistories.Add(InventoryPriceHistory.Create(
            inventory.InventoryId,
            current.Price,
            current.OriginalPrice,
            current.DiscountPercentage,
            current.Availability,
            inventory.ObservedAtUtc ?? DateTime.UtcNow));
    }

    // An offer created after the migration has no observation yet, so its series would stay empty until the
    // first price change. Recording the creation point as the baseline keeps the series starting at creation,
    // which is what the seeded baseline does for offers that already existed.
    public static void RecordBaseline(IApplicationDbContext context, Inventory inventory)
    {
        var current = InventoryPricePoint.Capture(inventory);
        context.InventoryPriceHistories.Add(InventoryPriceHistory.Create(
            inventory.InventoryId,
            current.Price,
            current.OriginalPrice,
            current.DiscountPercentage,
            current.Availability,
            inventory.ObservedAtUtc ?? DateTime.UtcNow,
            isBaseline: true));
    }
}
