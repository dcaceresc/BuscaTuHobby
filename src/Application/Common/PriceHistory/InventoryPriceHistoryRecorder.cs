namespace Application.Common.PriceHistory;

// Los cuatro campos que componen una observación del precio de una oferta.
public readonly record struct InventoryPricePoint(int Price, int OriginalPrice, int DiscountPercentage, bool? Availability)
{
    public static InventoryPricePoint Capture(Inventory inventory) =>
        new(inventory.Price, inventory.OriginalPrice, inventory.DiscountPercentage, inventory.Availability);
}

// El único lugar donde crece la serie de precios: se añade un punto solo cuando uno de los cuatro campos
// de precio difiere de su valor previo a la mutación, así que las observaciones idénticas repetidas nunca inflan la serie.
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

    // Una oferta creada después de la migración todavía no tiene observación, así que su serie quedaría vacía
    // hasta el primer cambio de precio. Registrar el punto de creación como línea base hace que la serie empiece
    // en la creación, que es lo que hace la línea base sembrada para las ofertas que ya existían.
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
