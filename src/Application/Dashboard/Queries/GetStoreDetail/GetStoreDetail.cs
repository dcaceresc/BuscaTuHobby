namespace Application.Dashboard.Queries.GetStoreDetail;

public record GetStoreDetail(string Slug) : IRequest<ApiResponse<StoreDetailDto>>;

public class GetStoreDetailHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetStoreDetail, ApiResponse<StoreDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<StoreDetailDto>> Handle(GetStoreDetail request, CancellationToken cancellationToken)
    {
        try
        {
            // El slug se compara en minúsculas por la misma razón que en la categoría: la ruta lo publica así y la
            // base puede tenerlo escrito de otra forma. Una tienda inactiva no existe para el visitante.
            var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();

            var store = await _context.Stores
                .AsNoTracking()
                .Where(s => s.IsActive && s.StoreSlug.ToLower() == slug)
                .Select(s => new { s.StoreId, s.StoreName, s.StoreSlug, s.StoreIcon, s.StoreWebSite })
                .SingleOrDefaultAsync(cancellationToken);

            if (store is null)
                return _responseService.Fail<StoreDetailDto>("La tienda no existe o no está disponible.");

            // La misma regla de elegibilidad que el detalle de producto: la tienda está activa porque se acaba de
            // resolver, y la oferta, el producto y la disponibilidad se comprueban por oferta.
            var offers = await _context.Inventories
                .AsNoTracking()
                .Where(i => i.StoreId == store.StoreId && i.IsActive && i.Store.IsActive &&
                            i.Product.IsActive && i.Availability != false)
                .OrderBy(i => i.Price)
                .ThenBy(i => i.Product.ProductName)
                .ThenBy(i => i.InventoryId)
                .Select(i => new StoreOfferDto
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.ProductName,
                    Price = i.Price,
                    OriginalPrice = i.OriginalPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    Condition = (int)i.Condition,
                    PaymentMethodLabel = i.PaymentMethodLabel,
                    ObservedAtUtc = i.ObservedAtUtc
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(new StoreDetailDto
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                StoreSlug = store.StoreSlug,
                StoreIcon = store.StoreIcon,
                StoreWebSite = store.StoreWebSite,
                // Los dos conteos describen las ofertas publicadas: un producto puede repetirse en ofertas, así que
                // el conteo de productos es por producto distinto.
                ProductCount = offers.Select(o => o.ProductId).Distinct().Count(),
                OfferCount = offers.Count,
                Offers = offers
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<StoreDetailDto>("Ha ocurrido un error al obtener la tienda.");
        }
    }
}
