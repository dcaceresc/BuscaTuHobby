namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public class InventoryVM
{
    public Guid InventoryId { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public int Price { get; set; }
    public int OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }

    // Condición como entero (0 nueva, 1 refabricada, 2 usada) y medio de pago que muestra la pantalla de administración.
    public int Condition { get; set; }
    public string? PaymentMethodLabel { get; set; }
}
