namespace Application.Maintainer.Products.Commands.ToggleProduct;

public record ToggleProduct(Guid ProductId) : IRequest<ApiResponse>;

public class ToggleProductHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleProduct, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleProduct request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

            Guard.Against.NotFound(product, $"No existe producto con la Id {request.ProductId}");

            product.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Producto actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el producto");
        }
    }
}