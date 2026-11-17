namespace Application.Dashboard.Queries.GetPriceDrops;

// Days se acota igual que en el historial de precios de un producto: 0 conserva toda la serie. MinPercent se acota a
// 1..100 porque un umbral de 0 incluiría cualquier variación y uno negativo no describiría ninguna bajada.
public record GetPriceDrops(int Days = 30, int MinPercent = 10) : IRequest<ApiResponse<List<PriceDropDto>>>;

public class GetPriceDropsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetPriceDrops, ApiResponse<List<PriceDropDto>>>
{
    private const int MaxDays = 3650;
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<PriceDropDto>>> Handle(GetPriceDrops request, CancellationToken cancellationToken)
    {
        try
        {
            var days = Math.Clamp(request.Days, 0, MaxDays);
            var since = DateTime.UtcNow.AddDays(-days);

            // La misma elegibilidad que la ficha del producto: una oferta desactivada, de una tienda cerrada, de un
            // producto retirado o marcada como no disponible no publica una bajada que el visitante no puede comprar.
            var query = _context.InventoryPriceHistories
                .AsNoTracking()
                .Where(history => history.Inventory.IsActive && history.Inventory.Store.IsActive &&
                    history.Inventory.Product.IsActive && history.Inventory.Availability != false);
            if (days > 0)
                query = query.Where(history => history.ObservedAtUtc >= since);

            // La comparación de los dos últimos puntos no se puede expresar como una consulta portátil sin funciones de
            // ventana, así que la serie se filtra en la base y se compara en memoria sobre lo que la ventana devuelve.
            var points = await query
                .Select(history => new PriceDropPoint(
                    history.InventoryId,
                    history.PriceHistoryId,
                    history.Inventory.ProductId,
                    history.Inventory.Product.ProductName,
                    history.Inventory.Store.StoreName,
                    history.Inventory.Store.StoreSlug,
                    history.Inventory.SourceUrl,
                    history.Price,
                    history.ObservedAtUtc))
                .ToListAsync(cancellationToken);

            var drops = PriceDropSelection.Select(points, request.MinPercent);
            return _responseService.Success(drops.Select(PriceDropDto.From).ToList());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<List<PriceDropDto>>("Ha ocurrido un error al obtener las bajadas de precio.");
        }
    }
}
