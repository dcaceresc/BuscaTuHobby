using Application.Maintainer.Inventories.IrionOffers;

namespace Application.Maintainer.Inventories.Queries.InspectNewIrionProductOffer;

public record InspectNewIrionProductOffer(NewIrionProductDetails Product, Guid StoreId, string SourceUrl, string ExternalVariantId)
    : IRequest<ApiResponse<IrionOfferInspectionDto>>;

public class InspectNewIrionProductOfferHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<InspectNewIrionProductOffer, ApiResponse<IrionOfferInspectionDto>>
{
    public async Task<ApiResponse<IrionOfferInspectionDto>> Handle(InspectNewIrionProductOffer request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await NewIrionProductValidation.InspectAsync(context, request.Product, request.StoreId,
                request.SourceUrl, request.ExternalVariantId, cancellationToken);
            return result.Error is null ? responseService.Success(result.Inspection!) : responseService.Fail<IrionOfferInspectionDto>(result.Error);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception) { return responseService.Fail<IrionOfferInspectionDto>("No se pudo inspeccionar el producto nuevo de Irion."); }
    }
}
