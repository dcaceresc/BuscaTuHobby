namespace Application.Maintainer.Inventories.Queries.GetInventories;

public class InventoryDto
{
    public Guid InventoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StoreName { get; set; } = default!;
    public string Price { get; set; } = default!;
    public bool IsActive { get; set; }

    // Condición como entero (0 nueva, 1 refabricada, 2 usada) y medio de pago informado por la oferta.
    public int Condition { get; set; }
    public string? PaymentMethodLabel { get; set; }
}