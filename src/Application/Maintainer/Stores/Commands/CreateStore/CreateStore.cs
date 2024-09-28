namespace Application.Maintainer.Stores.Commands.CreateStore;

public record CreateStore(string StoreName, string StoreWebSite) : IRequest<ApiResponse<Guid>>;

public class CreateStoreHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateStore, ApiResponse<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<Guid>> Handle(CreateStore request, CancellationToken cancellationToken)
    {
        try
        {
            var store = Store.Create(request.StoreName, request.StoreWebSite);

            _context.Stores.Add(store);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success(store.StoreId);
        }
        catch (Exception)
        {
            return _responseService.Fail<Guid>("Error al cerrar la tienda");
        }
    }
}