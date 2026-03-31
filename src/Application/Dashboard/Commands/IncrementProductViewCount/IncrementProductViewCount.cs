namespace Application.Dashboard.Commands.IncrementProductViewCount;

public record IncrementProductViewCount(Guid ProductId) : IRequest<ApiResponse>;

public class IncrementProductViewCountHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<IncrementProductViewCount, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(IncrementProductViewCount request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

            Guard.Against.NotFound(product, $"No existe un producto con la Id {request.ProductId}");

            product.IncrementViewCount();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Vista registrada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrio un error al registrar la vista del producto");
        }
    }
}
