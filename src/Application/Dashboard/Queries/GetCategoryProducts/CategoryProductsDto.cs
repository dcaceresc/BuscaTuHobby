namespace Application.Dashboard.Queries.GetCategoryProducts;

// Respuesta del listado público de una categoría: la página pedida y los tres facets que describen la categoría,
// para que el frontend pinte los filtros sin una segunda petición.
public class CategoryProductsDto
{
    public string CategoryName { get; set; } = default!;
    public string CategorySlug { get; set; } = default!;
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CatalogProductDto> Items { get; set; } = [];
    public List<FilterOptionDto> Manufacturers { get; set; } = [];
    public List<FilterOptionDto> Franchises { get; set; } = [];
    public List<FilterOptionDto> Grades { get; set; } = [];
}

// Fila del listado. Los precios, el conteo de tiendas y las imágenes describen solo las ofertas elegibles del
// producto: la misma regla que aplican el detalle de producto y la lista de ofertas.
public class CatalogProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ManufacturerName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;

    // Grado como entero (0 sin grado ... 6 otro) para respetar el contrato HTTP sin JsonStringEnumConverter.
    public int Grade { get; set; }
    public string? Scale { get; set; }
    public string? PartNumber { get; set; }
    // Precio mínimo y máximo entre las ofertas elegibles del producto. Son opcionales a propósito: null significa
    // que el producto no tiene ninguna oferta elegible y no se publica un cero inventado; un cero real sigue
    // siendo un precio publicado.
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
    public int StoreCount { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}

// Opción de un facet con la cantidad de productos de la categoría que la usan, para que el frontend muestre el
// conteo real en lugar de deducirlo de la página.
public class FilterOptionDto
{
    public string Value { get; set; } = default!;
    public string Label { get; set; } = default!;
    public int Count { get; set; }
}
