namespace Domain.Entities;

public class ProductImage : AuditableEntity
{
    private ProductImage(int productImageOrder, Guid productId)
    {
        ProductImageId = Guid.NewGuid();
        ProductImageOrder = productImageOrder;
        ProductId = productId;
    }

    public Guid ProductImageId { get; set; }
    public int ProductImageOrder { get; set; }
    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;

    public static ProductImage Create(int productImageOrder, Guid productId)
    {
        return new ProductImage(productImageOrder, productId);
    }

    public void Update(int productImageOrder, Guid productId)
    {
        ProductImageOrder = productImageOrder;
        ProductId = productId;
    }

}
