namespace Domain.Entities;

public class ProductCategory : AuditableEntity
{
    public int productId { get; set; }
    public Product Product { get; set; } = default!;
    public int categoryId { get; set; }
    public Category Category { get; set; } = default!;
}