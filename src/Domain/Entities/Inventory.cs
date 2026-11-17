namespace Domain.Entities;

public class Inventory : AuditableEntity
{
    private Inventory(Guid storeId, Guid productId, int price, int originalPrice, int discountPercentage,
        ProductCondition condition, string? paymentMethodLabel)
    {
        InventoryId = Guid.NewGuid();
        StoreId = storeId;
        ProductId = productId;
        Price = price;
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
        Condition = condition;
        PaymentMethodLabel = paymentMethodLabel;
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

    // "Nueva" es el valor por defecto porque las ofertas anteriores a este campo y las importadas no informan
    // condición: es la suposición más honesta y además el valor 0 que la migración asigna a las filas existentes.
    public ProductCondition Condition { get; private set; } = ProductCondition.New;
    public string? PaymentMethodLabel { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public virtual Product Product { get; private set; } = default!;
    public virtual Store Store { get; private set; } = default!;


    // Los parámetros nuevos son opcionales para no cambiar a ningún llamador existente: si se omiten, la oferta
    // queda como nueva y sin medio de pago, que es exactamente lo que ya ocurría antes de estos campos.
    public static Inventory Create(Guid productId, Guid storeId, int price, int originalPrice, int discountPercentage,
        ProductCondition condition = ProductCondition.New, string? paymentMethodLabel = null)
    {
        ValidateReferences(productId, storeId);
        ValidateOfferDetails(condition, paymentMethodLabel);
        return new Inventory(storeId, productId, price, originalPrice, discountPercentage, condition, paymentMethodLabel);
    }

    // Los mutadores exigen condición y medio de pago: al crearlos son opcionales porque una oferta nueva es
    // nueva, pero al modificarlos un valor por defecto borraría en silencio lo que cargó el administrador.
    public void Update(Guid productId, Guid storeId, int price, int originalPrice, int discountPercentage,
        ProductCondition condition, string? paymentMethodLabel)
    {
        ValidateReferences(productId, storeId);
        ValidateOfferDetails(condition, paymentMethodLabel);
        if (SourceUrl is not null && (ProductId != productId || StoreId != storeId))
            throw new InvalidOperationException("No se puede cambiar la asociación de una oferta importada.");
        StoreId = storeId;
        ProductId = productId;
        Price = price;
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
        Condition = condition;
        PaymentMethodLabel = paymentMethodLabel;
    }

    // Una oferta creada a mano no viene de una importación, pero su precio también se observa en una fecha:
    // sin ella la tabla de ofertas no puede mostrar frescura y el dato queda incompleto.
    public void MarkObserved(DateTime observedAtUtc)
    {
        if (observedAtUtc == default)
            throw new ArgumentException("La fecha de observación es obligatoria.", nameof(observedAtUtc));
        ObservedAtUtc = observedAtUtc;
    }

    // La aplicación valida la tienda y el producto elegidos y convierte los valores CLP enteros antes de llamar a estos métodos.
    public static Inventory CreateImportedOffer(Guid productId, Guid storeId, int price, int originalPrice,
        int discountPercentage, string sourceUrl, string externalVariantId, bool availability, DateTime observedAtUtc,
        ProductCondition condition = ProductCondition.New, string? paymentMethodLabel = null)
    {
        var inventory = Create(productId, storeId, price, originalPrice, discountPercentage, condition, paymentMethodLabel);
        ValidateImport(price, originalPrice, discountPercentage, sourceUrl, externalVariantId, observedAtUtc);
        inventory.SetObservation(sourceUrl, externalVariantId, availability, observedAtUtc);
        return inventory;
    }

    public void UpdateImportedOffer(int price, int originalPrice, int discountPercentage, string sourceUrl,
        string externalVariantId, bool availability, DateTime observedAtUtc,
        ProductCondition condition, string? paymentMethodLabel)
    {
        if (SourceUrl is null || SourceUrl != sourceUrl || ExternalVariantId != externalVariantId)
            throw new InvalidOperationException("La oferta ya tiene otra vinculación o no está vinculada.");
        ValidateReferences(ProductId, StoreId);
        ValidateImport(price, originalPrice, discountPercentage, sourceUrl, externalVariantId, observedAtUtc);
        Update(ProductId, StoreId, price, originalPrice, discountPercentage, condition, paymentMethodLabel);
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

    // La condición y la etiqueta se validan igual que el resto de los metadatos de la oferta: rechazo atómico con
    // un mensaje en español y sin mutar nada antes de comprobarlo.
    private static void ValidateOfferDetails(ProductCondition condition, string? paymentMethodLabel)
    {
        if (!OfferConditionRules.IsValidCondition((int)condition))
            throw new ArgumentException(OfferConditionRules.InvalidConditionMessage, nameof(condition));
        if (!OfferConditionRules.IsValidPaymentMethodLabel(paymentMethodLabel))
            throw new ArgumentException(OfferConditionRules.InvalidPaymentMethodLabelMessage, nameof(paymentMethodLabel));
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