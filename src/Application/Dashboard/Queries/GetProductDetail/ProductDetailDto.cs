namespace Application.Dashboard.Queries.GetProductDetail;

public class ProductDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public string ManufacturerName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;
    public string? SerieName { get; set; }
    public bool ProductHasBase { get; set; }
    public string ProductTargetAge { get; set; } = default!;
    public string ProductSize { get; set; } = default!;
    public DateOnly ProductReleaseDate { get; set; }
    public int ProductViewCount { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<string> ImageUrls { get; set; } = [];
    public int StoreCount { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }

    // Ordenadas de la tienda más barata a la más cara.
    public List<ProductOfferDto> Offers { get; set; } = [];
}

public class ProductOfferDto
{
    public Guid InventoryId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreSlug { get; set; } = default!;
    public string StoreIcon { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;
    public int Price { get; set; }
    public int OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public bool? Availability { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? ObservedAtUtc { get; set; }

    // Condición como entero (0 nueva, 1 refabricada, 2 usada) para respetar el contrato HTTP sin JsonStringEnumConverter.
    public int Condition { get; set; }
    public string? PaymentMethodLabel { get; set; }
}
