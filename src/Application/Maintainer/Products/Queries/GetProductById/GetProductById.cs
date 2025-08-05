namespace Application.Maintainer.Products.Queries.GetProductById;
public record GetProductById(Guid ProductId) : IRequest<ApiResponse<ProductVM>>;

public class GetProductByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetProductById, ApiResponse<ProductVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ProductVM>> Handle(GetProductById request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products
                           .Include(x => x.Manufacturer)
                           .Include(x => x.ProductCategories)
                           .Include(x => x.Franchise)
                           .ThenInclude(x => x.Series)
                           .Include(x => x.ProductImages)
                           .AsNoTracking()
                           .Select(x => new ProductVM()
                           {
                               ProductId = x.ProductId,
                               ProductName = x.ProductName,
                               CategoryIds = x.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                               ProductImages = x.ProductImages.OrderBy(pi => pi.ProductImageOrder).Select(pi => $"{SiteConfig.FolderImage}/{pi.ProductImageId}.jpg").ToList(),
                               FranchiseId = x.FranchiseId,
                               ManufacturerId = x.ManufacturerId,
                               SerieId = x.SerieId,
                               ProductDescription = x.ProductDescription,
                               ProductHasBase = x.ProductHasBase,
                               ProductReleaseDate = x.ProductReleaseDate.ToString("yyyy-MM-dd"),
                               ProductTargetAge = x.ProductTargetAge,
                               ProductSize = x.ProductSize
                           })
                           .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

            Guard.Against.NotFound(product, $"No existe producto con la Id {request.ProductId}");

            return _responseService.Success(product);
        }
        catch (Exception)
        {
            return _responseService.Fail<ProductVM>("An error occurred while processing your request");
        }
    }
}
