namespace Application.Maintainer.Stores.Commands.ToggleStore;

public record ToggleStore(Guid StoreId) : IRequest<ApiResponse>;

public class ToggleStoreHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleStore, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleStore request, CancellationToken cancellationToken)
    {
        try
        {
            var store = await _context.Stores.FindAsync([request.StoreId], cancellationToken);

            Guard.Against.NotFound(store, $"No existe tienda con la Id {request.StoreId}");

            store.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la tienda se actualizó correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la tienda");
        }
    }
}