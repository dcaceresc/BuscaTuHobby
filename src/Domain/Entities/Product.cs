namespace Domain.Entities;

public class Product : AuditableEntity
{
    private Product(string productName, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate, GunplaGrade grade, string? scale, string? partNumber)
    {
        ProductId = Guid.NewGuid();
        ProductName = productName;
        ManufacturerId = manufacturerId;
        FranchiseId = franchiseId;
        SerieId = serieId;
        ProductHasBase = productHasBase;
        ProductTargetAge = productTargetAge;
        ProductSize = productSize;
        ProductDescription = productDescription;
        ProductReleaseDate = productReleaseDate;
        Grade = grade;
        Scale = scale;
        PartNumber = partNumber;
        ProductViewCount = 0;
        IsActive = true;
    }


    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public Guid ManufacturerId { get; private set; }
    public Guid FranchiseId { get; private set; }
    public Guid? SerieId { get; private set; }
    public bool ProductHasBase { get; private set; }
    public string ProductTargetAge { get; private set; } = default!;
    public string ProductSize { get; private set; } = default!;
    public string ProductDescription { get; private set; } = default!;
    public DateOnly ProductReleaseDate { get; private set; }
    public int ProductViewCount { get; private set; }
    public bool IsActive { get; private set; }

    // Grado, escala y número de parte describen la maqueta. "Sin grado" es el valor por defecto porque las filas
    // anteriores a este campo no informan grado y el valor 0 es el que asigna la migración a las filas existentes.
    public GunplaGrade Grade { get; private set; } = GunplaGrade.None;
    public string? Scale { get; private set; }
    public string? PartNumber { get; private set; }

    public virtual Manufacturer Manufacturer { get; private set; } = default!;
    public virtual Franchise Franchise { get; private set; } = default!;
    public virtual Serie? Serie { get; private set; } = default!;

    public virtual ICollection<ProductImage> ProductImages { get; private set; } = default!;
    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; private set; } = default!;
    public virtual ICollection<ProductCategory> ProductCategories { get; private set; } = default!;
    public virtual ICollection<Inventory> Inventories { get; private set; } = default!;


    // Los parámetros de Gunpla son opcionales para no cambiar a ningún llamador existente: un producto creado sin
    // ellos queda sin grado, sin escala y sin número de parte, que es exactamente lo que ocurría antes de estos campos.
    public static Product Create(string productName, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate, GunplaGrade grade = GunplaGrade.None, string? scale = null, string? partNumber = null)
    {
        ValidateGunplaDetails(grade, scale, partNumber);
        return new Product(productName, manufacturerId, franchiseId, serieId, productHasBase, productTargetAge, productSize, productDescription, productReleaseDate, grade, scale, partNumber);
    }

    public static Product CreateWithProvidedId(Guid productId, string productName, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate, GunplaGrade grade = GunplaGrade.None, string? scale = null, string? partNumber = null)
    {
        if (productId == Guid.Empty) throw new ArgumentException("El identificador del producto es obligatorio.", nameof(productId));
        var product = Create(productName, manufacturerId, franchiseId, serieId, productHasBase, productTargetAge, productSize, productDescription, productReleaseDate, grade, scale, partNumber);
        product.ProductId = productId;
        return product;
    }

    // El mutador exige grado, escala y número de parte sin valor por defecto: al crearlos son opcionales porque un
    // producto nuevo puede no informarlos, pero al modificarlos un valor por defecto borraría en silencio lo que
    // cargó el administrador. Mismo criterio que Inventory.Update.
    public void Update(string productName, Guid manufacturerId, Guid franchiseId, Guid? serieId, bool productHasBase, string productTargetAge, string productSize, string productDescription, DateOnly productReleaseDate, GunplaGrade grade, string? scale, string? partNumber)
    {
        ValidateGunplaDetails(grade, scale, partNumber);
        ProductName = productName;
        ManufacturerId = manufacturerId;
        FranchiseId = franchiseId;
        SerieId = serieId;
        ProductHasBase = productHasBase;
        ProductTargetAge = productTargetAge;
        ProductSize = productSize;
        ProductDescription = productDescription;
        ProductReleaseDate = productReleaseDate;
        Grade = grade;
        Scale = scale;
        PartNumber = partNumber;
    }

    // El grado, la escala y el número de parte se validan antes de mutar nada, para que el rechazo sea atómico y
    // con el mismo mensaje en español que publican los casos de uso.
    private static void ValidateGunplaDetails(GunplaGrade grade, string? scale, string? partNumber)
    {
        if (!GunplaGradeRules.IsValidGrade((int)grade))
            throw new ArgumentException(GunplaGradeRules.InvalidGradeMessage, nameof(grade));
        if (!GunplaGradeRules.IsValidScale(scale))
            throw new ArgumentException(GunplaGradeRules.InvalidScaleMessage, nameof(scale));
        if (!GunplaGradeRules.IsValidPartNumber(partNumber))
            throw new ArgumentException(GunplaGradeRules.InvalidPartNumberMessage, nameof(partNumber));
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

    public void IncrementViewCount()
    {
        ProductViewCount++;
    }

    public void SetViewCount(int count)
    {
        ProductViewCount = count;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
