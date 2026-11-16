namespace Application.Dashboard.Queries.GetCategoryProducts;

// Listado público de una categoría con paginación, orden y filtros opcionales y combinables. La página y el tamaño
// llegan sin normalizar porque el cliente puede pedir cualquier valor y el caso de uso los acota.
public record GetCategoryProducts(string Slug, int? Page, int? PageSize, string? Sort, int? Grade,
    Guid? ManufacturerId, Guid? FranchiseId, int? MinPrice, int? MaxPrice) : IRequest<ApiResponse<CategoryProductsDto>>;

public class GetCategoryProductsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetCategoryProducts, ApiResponse<CategoryProductsDto>>
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 48;

    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<CategoryProductsDto>> Handle(GetCategoryProducts request, CancellationToken cancellationToken)
    {
        try
        {
            // El slug se compara en minúsculas: la ruta pública lo publica así y la base puede tenerlo escrito de otra
            // forma. Un slug en blanco simplemente no encuentra categoría y cae en el mismo fallo tipado.
            var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();

            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.CategorySlug.ToLower() == slug)
                .Select(c => new { c.CategoryId, c.CategoryName, c.CategorySlug })
                .SingleOrDefaultAsync(cancellationToken);

            if (category is null)
                return _responseService.Fail<CategoryProductsDto>("La categoría no existe o no está disponible.");

            // Universo de la consulta: los productos activos de la categoría. Los tres facets se calculan sobre este
            // universo sin los filtros del cliente, así cada opción informa cuántos productos de la categoría la usan
            // aunque el visitante ya tenga otro filtro puesto.
            var catalog = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.ProductCategories.Any(pc => pc.CategoryId == category.CategoryId));

            var manufacturerRows = await catalog
                .GroupBy(p => new { p.ManufacturerId, p.Manufacturer.ManufacturerName })
                .Select(g => new { g.Key.ManufacturerId, g.Key.ManufacturerName, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var franchiseRows = await catalog
                .GroupBy(p => new { p.FranchiseId, p.Franchise.FranchiseName })
                .Select(g => new { g.Key.FranchiseId, g.Key.FranchiseName, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var gradeRows = await catalog
                .GroupBy(p => p.Grade)
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // El valor publicado es el identificador de la fila, que es lo que aceptan los filtros; la etiqueta es el
            // nombre que se muestra. El orden es el de uso real: primero la opción con más productos.
            var manufacturers = manufacturerRows
                .OrderByDescending(r => r.Count).ThenBy(r => r.ManufacturerName, StringComparer.Ordinal)
                .Select(r => new FilterOptionDto
                {
                    Value = r.ManufacturerId.ToString(),
                    Label = r.ManufacturerName,
                    Count = r.Count
                })
                .ToList();

            var franchises = franchiseRows
                .OrderByDescending(r => r.Count).ThenBy(r => r.FranchiseName, StringComparer.Ordinal)
                .Select(r => new FilterOptionDto
                {
                    Value = r.FranchiseId.ToString(),
                    Label = r.FranchiseName,
                    Count = r.Count
                })
                .ToList();

            var grades = gradeRows
                .OrderByDescending(r => r.Count).ThenBy(r => (int)r.Grade)
                .Select(r => new FilterOptionDto
                {
                    Value = ((int)r.Grade).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Label = GunplaGradeRules.LabelOf(r.Grade),
                    Count = r.Count
                })
                .ToList();

            var filtered = catalog;

            if (request.Grade is { } grade)
                filtered = filtered.Where(p => (int)p.Grade == grade);

            if (request.ManufacturerId is { } manufacturerId)
                filtered = filtered.Where(p => p.ManufacturerId == manufacturerId);

            if (request.FranchiseId is { } franchiseId)
                filtered = filtered.Where(p => p.FranchiseId == franchiseId);

            // Los filtros de precio usan el precio más barato entre las ofertas elegibles. Un producto sin ofertas
            // elegibles no tiene precio que comparar, así que queda fuera de cualquier rango en lugar de entrar con 0.
            if (request.MinPrice is { } minPrice)
                filtered = filtered.Where(p => p.Inventories
                    .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                    .Select(i => (int?)i.Price).Min() >= minPrice);

            if (request.MaxPrice is { } maxPrice)
                filtered = filtered.Where(p => p.Inventories
                    .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                    .Select(i => (int?)i.Price).Min() <= maxPrice);

            // La página empieza en 1 y el tamaño se acota a 20 por defecto y a 1..48 cuando el cliente lo pide.
            var page = request.Page is { } requestedPage && requestedPage > 1 ? requestedPage : 1;
            var pageSize = request.PageSize is { } requestedPageSize
                ? Math.Min(Math.Max(requestedPageSize, 1), MaxPageSize)
                : DefaultPageSize;

            // El total se cuenta antes de paginar para que el frontend pueda mostrar cuántas páginas hay.
            var total = await filtered.CountAsync(cancellationToken);

            var rows = filtered.Select(p => new
            {
                p.ProductId,
                p.ProductName,
                ManufacturerName = p.Manufacturer.ManufacturerName,
                FranchiseName = p.Franchise.FranchiseName,
                Grade = (int)p.Grade,
                p.Scale,
                p.PartNumber,
                p.ProductViewCount,
                Cheapest = p.Inventories
                    .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                    .Select(i => (int?)i.Price).Min(),
                Dearest = p.Inventories
                    .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                    .Select(i => (int?)i.Price).Max(),
                StoreCount = p.Inventories
                    .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                    .Select(i => i.StoreId).Distinct().Count(),
                ImageIds = p.ProductImages
                    .OrderBy(pi => pi.ProductImageOrder)
                    .Select(pi => pi.ProductImageId)
                    .ToList()
            });

            // "name" es el orden predeterminado y cualquier valor desconocido cae en él en lugar de romper la página.
            // En los dos órdenes de precio la primera clave separa los productos sin ofertas elegibles para que queden
            // al final; el nombre desempata para que el listado sea estable entre páginas.
            var sort = (request.Sort ?? string.Empty).Trim().ToLowerInvariant();
            var sorted = sort switch
            {
                "price-asc" => rows
                    .OrderBy(r => r.Cheapest == null).ThenBy(r => r.Cheapest).ThenBy(r => r.ProductName),
                "price-desc" => rows
                    .OrderBy(r => r.Cheapest == null).ThenByDescending(r => r.Cheapest).ThenBy(r => r.ProductName),
                "views" => rows
                    .OrderByDescending(r => r.ProductViewCount).ThenBy(r => r.ProductName),
                _ => rows.OrderBy(r => r.ProductName)
            };

            // El salto se calcula en 64 bits para que una página enorme no desborde; pedir más allá del total
            // simplemente devuelve la lista de productos vacía con el total real.
            var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);

            var pageRows = await sorted
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = pageRows.Select(r => new CatalogProductDto
            {
                ProductId = r.ProductId,
                ProductName = r.ProductName,
                ManufacturerName = r.ManufacturerName,
                FranchiseName = r.FranchiseName,
                Grade = r.Grade,
                Scale = r.Scale,
                PartNumber = r.PartNumber,
                MinPrice = r.Cheapest ?? 0,
                MaxPrice = r.Dearest ?? 0,
                StoreCount = r.StoreCount,
                ImageUrls = r.ImageIds.Select(id => $"/images/{id}.jpg").ToList()
            }).ToList();

            return _responseService.Success(new CategoryProductsDto
            {
                CategoryName = category.CategoryName,
                CategorySlug = category.CategorySlug,
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items,
                Manufacturers = manufacturers,
                Franchises = franchises,
                Grades = grades
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<CategoryProductsDto>("Ha ocurrido un error al obtener los productos de la categoría.");
        }
    }
}
