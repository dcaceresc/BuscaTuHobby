namespace Domain.Entities;

public class Product : AuditableEntity
{
    private Product(string productName, Guid scaleId, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate)
    {
        ProductId = Guid.NewGuid();
        ProductName = productName;
        ScaleId = scaleId;
        ManufacturerId = manufacturerId;
        FranchiseId = franchiseId;
        SerieId = serieId;
        ProductHasBase = productHasBase;
        ProductTargetAge = productTargetAge;
        ProductSize = productSize;
        ProductDescription = productDescription;
        ProductReleaseDate = productReleaseDate;
        IsActive = true;
    }


    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public Guid ScaleId { get; private set; }
    public Guid ManufacturerId { get; private set; }
    public Guid FranchiseId { get; private set; }
    public Guid? SerieId { get; private set; }
    public bool ProductHasBase { get; private set; }
    public string ProductTargetAge { get; private set; } = default!;
    public string ProductSize { get; private set; } = default!;
    public string ProductDescription { get; private set; } = default!;
    public DateOnly ProductReleaseDate { get; private set; }
    public bool IsActive { get; private set; }

    public virtual Manufacturer Manufacturer { get; private set; } = default!;
    public virtual Scale Scale { get; private set; } = default!;
    public virtual Franchise Franchise { get; private set; } = default!;
    public virtual Serie? Serie { get; private set; } = default!;

    public virtual ICollection<ProductImage> ProductImages { get; private set; } = default!;
    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; private set; } = default!;
    public virtual ICollection<ProductCategory> ProductCategories { get; private set; } = default!;


    public static Product Create(string productName, Guid scaleId, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate)
    {
        return new Product(productName, scaleId, manufacturerId, franchiseId, serieId, productHasBase, productTargetAge, productSize, productDescription, productReleaseDate);
    }

    public void Update(string productName, Guid scaleId, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate)
    {
        ProductName = productName;
        ScaleId = scaleId;
        ManufacturerId = manufacturerId;
        FranchiseId = franchiseId;
        SerieId = serieId;
        ProductHasBase = productHasBase;
        ProductTargetAge = productTargetAge;
        ProductSize = productSize;
        ProductDescription = productDescription;
        ProductReleaseDate = productReleaseDate;
    }

    public ProductCategory AssignCategory(Guid categoryId)
    {
        var categoryProduct = ProductCategory.Create(ProductId, categoryId);

        return categoryProduct;
    }

    public ProductImage AssignImage(int productImageOrder)
    {
        var productImage = ProductImage.Create(productImageOrder, ProductId);

        return productImage;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
