using Application.Maintainer.Inventories.IrionOffers;

namespace Application.Maintainer.Inventories.Queries.GetIrionNewProductOptions;

public record GetIrionNewProductOptions : IRequest<ApiResponse<NewIrionProductOptionsDto>>;

public class GetIrionNewProductOptionsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetIrionNewProductOptions, ApiResponse<NewIrionProductOptionsDto>>
{
    public async Task<ApiResponse<NewIrionProductOptionsDto>> Handle(GetIrionNewProductOptions request, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var manufacturers = await context.Manufacturers.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ManufacturerName)
                .Select(x => new NewIrionCatalogOption(x.ManufacturerId, x.ManufacturerName)).ToListAsync(cancellationToken);
            var franchises = await context.Franchises.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.FranchiseName)
                .Select(x => new NewIrionCatalogOption(x.FranchiseId, x.FranchiseName)).ToListAsync(cancellationToken);
            var categories = await context.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.CategoryOrder).ThenBy(x => x.CategoryName)
                .Select(x => new NewIrionCatalogOption(x.CategoryId, x.CategoryName)).ToListAsync(cancellationToken);
            var series = await context.Series.AsNoTracking().Where(x => x.IsActive && context.Franchises.Any(f => f.FranchiseId == x.FranchiseId && f.IsActive))
                .OrderBy(x => x.SerieName).Select(x => new NewIrionSerieOption(x.SerieId, x.SerieName, x.FranchiseId)).ToListAsync(cancellationToken);
            return responseService.Success(new NewIrionProductOptionsDto(manufacturers, franchises, categories, series));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception) { return responseService.Fail<NewIrionProductOptionsDto>("No se pudieron obtener las opciones del producto nuevo."); }
    }
}
