
namespace Application.Maintainer.Products.Commands.CreateProductImages;
public record CreateProductImages(Guid ProductId, List<string> Images) : IRequest<ApiResponse>;

public class CreateProductImagesHandler(IApplicationDbContext context, IApiResponseService responseService, IUtilityService utilityService) : IRequestHandler<CreateProductImages, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IUtilityService _utilityService = utilityService;

    public async Task<ApiResponse> Handle(CreateProductImages request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

            Guard.Against.NotFound(product, $"No existe producto con la Id {request.ProductId}");

            for (var i = 0; i < request.Images.Count; i++)
            {
                var image = product.AssignImage(i);

                _context.ProductImages.Add(image);

                await _context.SaveChangesAsync(cancellationToken);

                await _utilityService.SaveImagen(request.Images[i], image.ProductImageId);
            }

            return _responseService.Success("Se crearon las imágenes del producto correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudieron crear las imágenes del producto");
        }
    }
}
