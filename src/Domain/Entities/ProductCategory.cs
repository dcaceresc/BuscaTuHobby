namespace Domain.Entities;

public class ProductCategory : AuditableEntity
{
    private ProductCategory(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        CategoryId = categoryId;
    }

    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;
    public Guid CategoryId { get; set; } = default!;
    public Category Category { get; set; } = default!;

    public static ProductCategory Create(Guid productId, Guid categoryId)
    {
        return new ProductCategory(productId, categoryId);
    }
}