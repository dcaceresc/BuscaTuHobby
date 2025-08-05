namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public record GetInventoryById(Guid InventoryId) : IRequest<ApiResponse<InventoryVM>>;

public class GetInventoryByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetInventoryById, ApiResponse<InventoryVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<InventoryVM>> Handle(GetInventoryById request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _context.Inventories
               .AsNoTracking()
               .Select(x => new InventoryVM
               {
                   InventoryId = x.InventoryId,
                   ProductId = x.ProductId,
                   StoreId = x.StoreId,
                   Price = x.Price,
               })
               .FirstOrDefaultAsync(x => x.InventoryId == request.InventoryId, cancellationToken);

            Guard.Against.NotFound(inventory, $"No existe inventario con Id {request.InventoryId}");

            return _responseService.Success(inventory);
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail<InventoryVM>(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<InventoryVM>("No se pudo obtener el inventario");
        }
    }
}
