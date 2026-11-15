using Application.Common.Exceptions;

namespace Application.Maintainer.Products.Queries.PreviewIrionProduct;

public record PreviewIrionProduct(string Url) : IRequest<ApiResponse<IrionProductPreviewDto>>;

public class PreviewIrionProductHandler(
    IIrionProductExtractor extractor,
    IApiResponseService responseService)
    : IRequestHandler<PreviewIrionProduct, ApiResponse<IrionProductPreviewDto>>
{
    private readonly IIrionProductExtractor _extractor = extractor;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<IrionProductPreviewDto>> Handle(
        PreviewIrionProduct request, CancellationToken cancellationToken)
    {
        try
        {
            var preview = await _extractor.ExtractAsync(request.Url, cancellationToken);
            return _responseService.Success(preview);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (IrionProductUrlException exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(exception.Message);
        }
        catch (IrionRateLimitException exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(exception.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<IrionProductPreviewDto>(
                "No se pudo obtener la vista previa del producto de Irion.");
        }
    }
}
