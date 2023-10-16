namespace Domain.Entities;

public class SubCategoryProduct
{
    public int productId { get; set; }
    public Product Product { get; set; } = default!;
    public int subCategoryId { get; set; }
    public SubCategory SubCategory { get; set; } = default!;
}