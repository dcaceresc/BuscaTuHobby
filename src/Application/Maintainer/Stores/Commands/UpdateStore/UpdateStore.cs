namespace Application.Maintainer.Stores.Commands.UpdateStore;

public record UpdateStore : IRequest<ApiResponse>
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = default!;
    public string StoreAddress { get; init; } = default!;
    public string StoreWebSite { get; init; } = default!;

}

public class UpdateStoreHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateStore, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateStore request, CancellationToken cancellationToken)
    {
        try
        {
            var store = await _context.Stores.FindAsync([request.StoreId], cancellationToken);

            Guard.Against.NotFound(store, $"No existe tienda con la Id {request.StoreId}");

            store.Update(request.StoreName, request.StoreAddress, request.StoreWebSite);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La tienda se actualizó correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la tienda");
        }


    }
}
