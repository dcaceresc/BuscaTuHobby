namespace Application.Maintainer.Stores.Commands.DeleteStoreAddress;
public record DeleteStoreAddress (Guid StoreId, Guid StoreAddressId) : IRequest<ApiResponse>;

public class DeleteStoreAddressHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<DeleteStoreAddress, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(DeleteStoreAddress request, CancellationToken cancellationToken)
    {
        try
        {
            var storeAddress = await _context.StoresAddresses
                .Where(x => x.StoreId == request.StoreId && x.StoreAddressId == request.StoreAddressId)
                .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(storeAddress, "No existe dirección de la tienda");

            _context.StoresAddresses.Remove(storeAddress);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La dirección de la tienda ha sido eliminada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al eliminar la dirección de la tienda");
        }
    }
}
