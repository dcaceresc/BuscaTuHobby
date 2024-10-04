namespace Application.Maintainer.Stores.Commands.UpdateStoreAddress;
public record UpdateStoreAddress(Guid StoreAddressId, Guid StoreId, string Street, Guid CommuneId, string? ZipCode) : IRequest<ApiResponse>;

public class UpdateStoreAddressHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateStoreAddress, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateStoreAddress request, CancellationToken cancellationToken)
    {
        try
        {
            var storeAddress = await _context.StoresAddresses.FindAsync([request.StoreAddressId], cancellationToken);

            Guard.Against.NotFound(storeAddress, $"No existe dirección de la tienda con Id {request.StoreAddressId}");

            storeAddress.Update(request.Street, request.StoreId, request.CommuneId, request.ZipCode);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La dirección de la tienda se actualizó correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar la dirección de la tienda");
        }
    }
}
