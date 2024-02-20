namespace Domain.Entities;

public class ProductImage : AuditableEntity
{
    private ProductImage(int productImageOrder, string productImagePath, Guid productId)
    {
        ProductImageId = Guid.NewGuid();
        ProductImageOrder = productImageOrder;
        ProductImagePath = productImagePath;
        ProductId = productId;
    }


    public Guid ProductImageId { get; set; }
    public int ProductImageOrder { get; set; }
    public string ProductImagePath { get; set; } = default!;
    public Guid ProductId { get; set; } = default!;
    public Product Product { get; set; } = default!;

    public static ProductImage Create(int productImageOrder, string productImagePath, Guid productId)
    {
        return new ProductImage(productImageOrder, productImagePath, productId);
    }

    public void Update(int productImageOrder, string productImagePath, Guid productId)
    {
        ProductImageOrder = productImageOrder;
        ProductImagePath = productImagePath;
        ProductId = productId;
    }

}
