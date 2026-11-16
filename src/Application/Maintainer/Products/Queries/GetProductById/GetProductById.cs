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
            // La carpeta física no forma parte de la URL pública: /images/{id}.jpg se mapea a la carpeta
            // configurada mediante el middleware de archivos estáticos de Program.cs.
            var row = await _context.Products
                           .AsNoTracking()
                           .Where(x => x.ProductId == request.ProductId)
                           .Select(x => new
                           {
                               Detail = new ProductVM()
                               {
                                   ProductId = x.ProductId,
                                   ProductName = x.ProductName,
                                   CategoryIds = x.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                                   FranchiseId = x.FranchiseId,
                                   ManufacturerId = x.ManufacturerId,
                                   SerieId = x.SerieId,
                                   ProductDescription = x.ProductDescription,
                                   ProductHasBase = x.ProductHasBase,
                                   ProductReleaseDate = x.ProductReleaseDate.ToString("yyyy-MM-dd"),
                                   ProductTargetAge = x.ProductTargetAge,
                                   ProductSize = x.ProductSize,
                                   Grade = (int)x.Grade,
                                   Scale = x.Scale,
                                   PartNumber = x.PartNumber
                               },
                               ImageIds = x.ProductImages
                                   .OrderBy(pi => pi.ProductImageOrder)
                                   .Select(pi => pi.ProductImageId)
                                   .ToList()
                           })
                           .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(row, $"No existe producto con la Id {request.ProductId}");

            row.Detail.ProductImages = row.ImageIds.Select(imageId => $"/images/{imageId}.jpg").ToList();

            return _responseService.Success(row.Detail);
        }
        catch (Exception)
        {
            return _responseService.Fail<ProductVM>("An error occurred while processing your request");
        }
    }
}
