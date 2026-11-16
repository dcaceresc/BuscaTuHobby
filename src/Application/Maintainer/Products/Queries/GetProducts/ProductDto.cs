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

    // Grado como entero (0 sin grado ... 6 otro) para respetar el contrato HTTP sin JsonStringEnumConverter.
    public int Grade { get; set; }
    public string? Scale { get; set; }
    public string? PartNumber { get; set; }

}