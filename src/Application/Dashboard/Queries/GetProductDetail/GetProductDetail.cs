namespace Application.Dashboard.Queries.GetProductDetail;

public record GetProductDetail(Guid ProductId) : IRequest<ApiResponse<ProductDetailDto>>;

public class GetProductDetailHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetProductDetail, ApiResponse<ProductDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ProductDetailDto>> Handle(GetProductDetail request, CancellationToken cancellationToken)
    {
        try
        {
            // Las URL de las imágenes se construyen después de la materialización: la conversión de identificador a ruta no es una operación SQL.
            var row = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == request.ProductId && p.IsActive)
                .Select(p => new
                {
                    Detail = new ProductDetailDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ProductDescription = p.ProductDescription,
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        FranchiseName = p.Franchise.FranchiseName,
                        SerieName = p.Serie == null ? null : p.Serie.SerieName,
                        ProductHasBase = p.ProductHasBase,
                        ProductTargetAge = p.ProductTargetAge,
                        ProductSize = p.ProductSize,
                        Grade = (int)p.Grade,
                        Scale = p.Scale,
                        PartNumber = p.PartNumber,
                        ProductReleaseDate = p.ProductReleaseDate,
                        ProductViewCount = p.ProductViewCount,
                        Categories = p.ProductCategories.Select(pc => pc.Category.CategoryName).ToList()
                    },
                    ImageIds = p.ProductImages
                        .OrderBy(pi => pi.ProductImageOrder)
                        .Select(pi => pi.ProductImageId)
                        .ToList()
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (row is null)
                return _responseService.Fail<ProductDetailDto>("El producto no existe o no está disponible.");

            var product = row.Detail;
            product.ImageUrls = row.ImageIds.Select(id => $"/images/{id}.jpg").ToList();

            // Primero la más barata: el orden lo aplica la base de datos, no el consumidor.
            product.Offers = await _context.Inventories
                .AsNoTracking()
                .Where(i => i.ProductId == request.ProductId && i.IsActive && i.Store.IsActive &&
                            i.Product.IsActive && i.Availability != false)
                .OrderBy(i => i.Price)
                .ThenBy(i => i.Store.StoreName)
                .Select(i => new ProductOfferDto
                {
                    InventoryId = i.InventoryId,
                    StoreId = i.StoreId,
                    StoreName = i.Store.StoreName,
                    StoreSlug = i.Store.StoreSlug,
                    StoreIcon = i.Store.StoreIcon,
                    StoreWebSite = i.Store.StoreWebSite,
                    Price = i.Price,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    Availability = i.Availability,
                    SourceUrl = i.SourceUrl,
                    ObservedAtUtc = i.ObservedAtUtc,
                    Condition = (int)i.Condition,
                    PaymentMethodLabel = i.PaymentMethodLabel
                })
                .ToListAsync(cancellationToken);

            product.StoreCount = product.Offers.Select(o => o.StoreId).Distinct().Count();
            product.MinPrice = product.Offers.Count == 0 ? 0 : product.Offers[0].Price;
            product.MaxPrice = product.Offers.Count == 0 ? 0 : product.Offers[^1].Price;

            return _responseService.Success(product);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<ProductDetailDto>("Ha ocurrido un error al obtener el producto.");
        }
    }
}
