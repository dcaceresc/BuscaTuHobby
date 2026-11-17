namespace Domain.Entities;

public class Store : AuditableEntity
{
    public const int MaximumConnectorKeyLength = 32;
    public const int MaximumConnectorSettingsLength = 4000;

    private Store(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        StoreId = Guid.NewGuid();
        StoreName = storeName;
        StoreIcon = storeIcon;
        StoreWebSite = storeWebSite;
        StoreOrder = storeOrder;
        StoreSlug = storeSlug;
        IsActive = true;
    }
    
    public Guid StoreId { get; private set; }
    public string StoreName { get; private set; } = default!;
    public string StoreIcon { get; private set; } = default!;
    public string StoreWebSite { get; private set; } = default!;
    public int StoreOrder { get; private set; }
    public string StoreSlug { get; private set; } = default!;
    public bool IsActive { get; private set; }

    // La plataforma desde la que se leen las ofertas (por ejemplo "shopify") y sus ajustes por tienda
    // (moneda, presupuesto de peticiones) son configuración de la fila: así una segunda tienda Shopify es
    // un alta más y no una clase más. Null significa que la tienda no tiene ingesta automática.
    public string? ConnectorKey { get; private set; }
    public string? ConnectorSettings { get; private set; }
    
    public virtual ICollection<Inventory> Inventories { get; private set; } = default!;
    public virtual ICollection<StoreAddress> StoreAddresses { get; private set; } = default!;

    public static Store Create(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        return new Store(storeName, storeWebSite, storeIcon, storeOrder, storeSlug);
    }

    public void Update(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        StoreName = storeName;
        StoreWebSite = storeWebSite;
        StoreIcon = storeIcon;
        StoreOrder = storeOrder;
        StoreSlug = storeSlug;
    }

    // Una clave vacía borra la configuración completa. Unos ajustes sin clave no tienen dueño, así que se
    // rechazan en vez de dejar una fila a medio configurar que nadie puede interpretar.
    public void ConfigureConnector(string? connectorKey, string? connectorSettings)
    {
        var key = connectorKey?.Trim();
        if (string.IsNullOrEmpty(key))
        {
            if (!string.IsNullOrWhiteSpace(connectorSettings))
                throw new ArgumentException("Los ajustes del conector requieren una clave de conector.");
            ConnectorKey = null;
            ConnectorSettings = null;
            return;
        }

        if (key.Length > MaximumConnectorKeyLength)
            throw new ArgumentException(
                $"La clave del conector no puede superar los {MaximumConnectorKeyLength} caracteres.");

        var settings = string.IsNullOrWhiteSpace(connectorSettings) ? null : connectorSettings.Trim();
        if (settings is not null && settings.Length > MaximumConnectorSettingsLength)
            throw new ArgumentException(
                $"Los ajustes del conector no pueden superar los {MaximumConnectorSettingsLength} caracteres.");

        ConnectorKey = key;
        ConnectorSettings = settings;
    }

    public StoreAddress AssignAddress(string street, Guid communeId, string? zipCode)
    {
        return StoreAddress.Create(street, StoreId, communeId, zipCode);
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
