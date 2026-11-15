namespace Domain.Entities;

public class Inventory : AuditableEntity
{
    private Inventory(Guid storeId, Guid productId, int price, int originalPrice, int discountPercentage)
    {
        InventoryId = Guid.NewGuid();
        StoreId = storeId;
        ProductId = productId;
        Price = price;
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
        IsActive = true;
    }

    public Guid InventoryId { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Price { get; private set; }
    public int OriginalPrice { get; private set; }
    public int DiscountPercentage { get; private set; }
    public bool IsActive { get; private set; }
    public string? SourceUrl { get; private set; }
    public string? ExternalVariantId { get; private set; }
    public bool? Availability { get; private set; }
    public DateTime? ObservedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public virtual Product Product { get; private set; } = default!;
    public virtual Store Store { get; private set; } = default!;


    public static Inventory Create(Guid productId, Guid storeId, int price, int originalPrice, int discountPercentage)
    {
        ValidateReferences(productId, storeId);
        return new Inventory(storeId, productId, price, originalPrice, discountPercentage);
    }

    public void Update(Guid productId, Guid storeId, int price, int originalPrice, int discountPercentage)
    {
        ValidateReferences(productId, storeId);
        if (SourceUrl is not null && (ProductId != productId || StoreId != storeId))
            throw new InvalidOperationException("No se puede cambiar la asociación de una oferta importada.");
        StoreId = storeId;
        ProductId = productId;
        Price = price;
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
    }

    // The application validates the selected store/product and converts whole CLP values before calling these methods.
    public static Inventory CreateImportedOffer(Guid productId, Guid storeId, int price, int originalPrice,
        int discountPercentage, string sourceUrl, string externalVariantId, bool availability, DateTime observedAtUtc)
    {
        var inventory = Create(productId, storeId, price, originalPrice, discountPercentage);
        ValidateImport(price, originalPrice, discountPercentage, sourceUrl, externalVariantId, observedAtUtc);
        inventory.SetObservation(sourceUrl, externalVariantId, availability, observedAtUtc);
        return inventory;
    }

    public void UpdateImportedOffer(int price, int originalPrice, int discountPercentage, string sourceUrl,
        string externalVariantId, bool availability, DateTime observedAtUtc)
    {
        if (SourceUrl is null || SourceUrl != sourceUrl || ExternalVariantId != externalVariantId)
            throw new InvalidOperationException("La oferta ya tiene otra vinculación o no está vinculada.");
        ValidateReferences(ProductId, StoreId);
        ValidateImport(price, originalPrice, discountPercentage, sourceUrl, externalVariantId, observedAtUtc);
        Update(ProductId, StoreId, price, originalPrice, discountPercentage);
        SetObservation(sourceUrl, externalVariantId, availability, observedAtUtc);
    }

    private void SetObservation(string sourceUrl, string externalVariantId, bool availability, DateTime observedAtUtc)
    {
        SourceUrl = sourceUrl;
        ExternalVariantId = externalVariantId;
        Availability = availability;
        ObservedAtUtc = observedAtUtc;
    }

    private static void ValidateReferences(Guid productId, Guid storeId)
    {
        if (productId == Guid.Empty || storeId == Guid.Empty)
            throw new ArgumentException("Producto y tienda son obligatorios.");
    }

    private static void ValidateImport(int price, int originalPrice, int discountPercentage, string sourceUrl,
        string externalVariantId, DateTime observedAtUtc)
    {
        if (price <= 0 || originalPrice < 0 || (originalPrice != 0 && originalPrice < price) ||
            discountPercentage is < 0 or > 100 || (originalPrice == 0 && discountPercentage != 0))
            throw new ArgumentException("Los precios o el descuento no son válidos.");
        if (string.IsNullOrWhiteSpace(sourceUrl) || sourceUrl.Length > 2048 ||
            !Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0 || sourceUrl != sourceUrl.Trim() ||
            string.IsNullOrWhiteSpace(externalVariantId) || externalVariantId.Length > 128 || externalVariantId != externalVariantId.Trim() ||
            observedAtUtc == default || observedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Los metadatos de la oferta no son válidos.");
        var expectedDiscount = originalPrice <= 0 ? 0 :
            (int)Math.Round((originalPrice - (decimal)price) * 100 / originalPrice, MidpointRounding.AwayFromZero);
        if (discountPercentage != expectedDiscount)
            throw new ArgumentException("El descuento no corresponde a los precios.");
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}