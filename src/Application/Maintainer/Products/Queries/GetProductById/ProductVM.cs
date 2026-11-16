namespace Application.Maintainer.Products.Queries.GetProductById;

public class ProductVM
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public Guid ManufacturerId { get; set; }
    public Guid FranchiseId { get; set; }
    public Guid? SerieId { get; set; }
    public bool ProductHasBase { get; set; }
    public string ProductTargetAge { get; set; } = default!;
    public string ProductSize { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public string ProductReleaseDate { get; set; } = default!;
    public IList<Guid> CategoryIds { get; set; } = default!;
    public IList<string> ProductImages { get; set; } = default!;

    // Grado, escala y número de parte del producto. El grado viaja como entero (0 sin grado ... 6 otro) para
    // respetar el contrato HTTP sin JsonStringEnumConverter.
    public int Grade { get; set; }
    public string? Scale { get; set; }
    public string? PartNumber { get; set; }
}