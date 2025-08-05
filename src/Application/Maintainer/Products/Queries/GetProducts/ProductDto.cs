namespace Application.Maintainer.Products.Queries.GetProducts;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ManufacturerName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;
    public string SerieName { get; set; } = default!;
    public IList<string> Categories { get; set; } = default!;
    public bool IsActive { get; set; }

}