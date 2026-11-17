using Application.Common.Ingestion;

namespace Application.Maintainer.Ingestion.Queries.GetIngestionStatus;

// El estado no recibe parámetros: describe las tiendas configuradas con lo que la ingesta dejó escrito.
public record GetIngestionStatus() : IRequest<ApiResponse<List<IngestionStatusDto>>>;

public class GetIngestionStatusHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetIngestionStatus, ApiResponse<List<IngestionStatusDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<IngestionStatusDto>>> Handle(
        GetIngestionStatus request, CancellationToken cancellationToken)
    {
        try
        {
            // Una tienda sin conector no tiene ingesta que vigilar: el estado lista sólo las configuradas, también la
            // que hoy está cerrada, porque saber que una tienda configurada no corre es justamente lo que se busca.
            var stores = await _context.Stores
                .AsNoTracking()
                .Where(store => store.ConnectorKey != null)
                .OrderBy(store => store.StoreOrder)
                .Select(store => new
                {
                    store.StoreId,
                    store.StoreName,
                    store.StoreSlug,
                    store.ConnectorKey,
                    store.IsActive
                })
                .ToListAsync(cancellationToken);

            var nowUtc = DateTime.UtcNow;
            // El catálogo es el mismo para todas las tiendas: la cobertura se mide contra el catálogo activo.
            var totalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(product => product.IsActive, cancellationToken);

            var status = new List<IngestionStatusDto>(stores.Count);
            foreach (var store in stores)
            {
                var runs = await _context.ImportRuns
                    .AsNoTracking()
                    .Where(run => run.StoreId == store.StoreId)
                    .ToListAsync(cancellationToken);

                // Una sola lectura por tienda: elegibilidad y frescura salen de las mismas filas de oferta.
                var offers = await _context.Inventories
                    .AsNoTracking()
                    .Where(offer => offer.StoreId == store.StoreId)
                    .Select(offer => new
                    {
                        offer.ProductId,
                        IsEligible = offer.IsActive && offer.Store.IsActive && offer.Product.IsActive &&
                            offer.Availability != false,
                        offer.ObservedAtUtc
                    })
                    .ToListAsync(cancellationToken);

                var metrics = IngestionMetrics.Compute(store.StoreName, runs,
                    offers.Select(offer => new IngestionOfferReading(offer.ProductId, offer.IsEligible,
                        offer.ObservedAtUtc)).ToList(), totalProducts, nowUtc);

                var row = IngestionStatusDto.From(metrics);
                row.StoreId = store.StoreId;
                row.StoreName = store.StoreName;
                row.StoreSlug = store.StoreSlug;
                row.ConnectorKey = store.ConnectorKey;
                row.IsActive = store.IsActive;
                status.Add(row);
            }

            return _responseService.Success(status);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return _responseService.Fail<List<IngestionStatusDto>>(
                "Ha ocurrido un error al obtener el estado de la ingesta.");
        }
    }
}
