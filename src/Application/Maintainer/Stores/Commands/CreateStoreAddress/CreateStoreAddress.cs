namespace Application.Maintainer.Stores.Commands.CreateStoreAddress;
public record CreateStoreAddress(Guid StoreId, string Street, Guid CommuneId, string? ZipCode) : IRequest<ApiResponse>;

public class CreateStoreAddressHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateStoreAddress, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateStoreAddress request, CancellationToken cancellationToken)
    {
        try
        {
            var storeAddress = StoreAddress.Create(request.Street, request.StoreId, request.CommuneId, request.ZipCode);

            _context.StoresAddresses.Add(storeAddress);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La dirección de tienda creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear la dirección de la tienda");
        }
    }
}
