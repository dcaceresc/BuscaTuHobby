namespace Application.Dashboard.Queries.GetProductPriceHistory;

// Days = 0 conserva toda la serie registrada; un valor positivo la limita a esa cantidad de días hacia atrás.
public record GetProductPriceHistory(Guid ProductId, int Days = 0) : IRequest<ApiResponse<ProductPriceHistoryDto>>;

public class GetProductPriceHistoryHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetProductPriceHistory, ApiResponse<ProductPriceHistoryDto>>
{
    private const int MaxDays = 3650;
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ProductPriceHistoryDto>> Handle(GetProductPriceHistory request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == request.ProductId && p.IsActive)
                .Select(p => new { p.ProductId, p.ProductName })
                .SingleOrDefaultAsync(cancellationToken);

            if (product is null)
                return _responseService.Fail<ProductPriceHistoryDto>("El producto no existe o no está disponible.");

            var days = Math.Clamp(request.Days, 0, MaxDays);
            // La serie responde qué costó este producto, no qué ofertas siguen vigentes: incluye todos los puntos
            // registrados para el producto, incluso los de una oferta hoy desactivada, de una tienda cerrada o con
            // Availability == false. Es una asimetría deliberada con la tabla de ofertas y la interfaz debe
            // explicarla: aquí el historial no se filtra por elegibilidad, solo por producto y por la ventana de días.
            var query = _context.InventoryPriceHistories
                .AsNoTracking()
                .Where(h => h.Inventory.ProductId == request.ProductId);
            if (days > 0)
            {
                var since = DateTime.UtcNow.AddDays(-days);
                query = query.Where(h => h.ObservedAtUtc >= since);
            }

            // Un punto por día: el precio observado más barato y el más caro, y cuántas ofertas se observaron.
            var points = await query
                .GroupBy(h => h.ObservedAtUtc.Date)
                .Select(g => new PricePointDto
                {
                    Date = g.Key,
                    MinPrice = g.Min(h => h.Price),
                    MaxPrice = g.Max(h => h.Price),
                    OfferCount = g.Select(h => h.InventoryId).Distinct().Count()
                })
                .OrderBy(p => p.Date)
                .ToListAsync(cancellationToken);

            return _responseService.Success(new ProductPriceHistoryDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                HasHistory = points.Count > 0,
                Points = points
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<ProductPriceHistoryDto>("Ha ocurrido un error al obtener el historial de precios.");
        }
    }
}
