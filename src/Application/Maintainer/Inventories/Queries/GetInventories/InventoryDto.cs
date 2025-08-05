namespace Application.Maintainer.Inventories.Queries.GetInventories;

public class InventoryDto
{
    public Guid InventoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StoreName { get; set; } = default!;
    public string Price { get; set; } = default!;
    public bool IsActive { get; set; }
}