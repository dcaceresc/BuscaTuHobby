namespace Application.Dashboard.Queries.GetStoreDetail;

// Ficha pública de una tienda: sus conteos y sus ofertas, siempre contados sobre las ofertas elegibles para que
// coincidan con lo que muestra el listado de ofertas del producto.
public class StoreDetailDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreSlug { get; set; } = default!;
    public string StoreIcon { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;
    public int ProductCount { get; set; }
    public int OfferCount { get; set; }

    // Ordenadas de la oferta más barata a la más cara.
    public List<StoreOfferDto> Offers { get; set; } = [];
}

public class StoreOfferDto
{
    public Guid InventoryId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Price { get; set; }
    public int OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }

    // Condición como entero (0 nueva, 1 refabricada, 2 usada) para respetar el contrato HTTP sin JsonStringEnumConverter.
    public int Condition { get; set; }
    public string? PaymentMethodLabel { get; set; }
    public DateTime? ObservedAtUtc { get; set; }
}
