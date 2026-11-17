using Application.Common.Exceptions;

namespace Application.Maintainer.Products.Queries.PreviewIrionProduct;

public record PreviewIrionProduct(string Url) : IRequest<ApiResponse<IrionProductPreviewDto>>;

public class PreviewIrionProductHandler(
    IStoreProductLoader loader,
    IApiResponseService responseService)
    : IRequestHandler<PreviewIrionProduct, ApiResponse<IrionProductPreviewDto>>
{
    private readonly IStoreProductLoader _loader = loader;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<IrionProductPreviewDto>> Handle(
        PreviewIrionProduct request, CancellationToken cancellationToken)
    {
        try
        {
            var preview = await _loader.LoadAsync(request.Url, cancellationToken);
            return _responseService.Success(preview);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (StoreOfferUrlException exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(exception.Message);
        }
        catch (StoreRateLimitException exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(exception.Message);
        }
        catch (StorePathDisallowedException exception)
        {
            // El portal de robots se respeta de forma visible: la ruta no se consultó y el motivo se devuelve.
            return _responseService.Fail<IrionProductPreviewDto>(exception.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(
                "No se pudo obtener la vista previa del producto de Irion.");
        }
    }
}
