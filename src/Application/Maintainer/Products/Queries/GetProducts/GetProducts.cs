namespace Application.Maintainer.Products.Queries.GetProducts;

public record GetProducts : IRequest<ApiResponse<List<ProductDto>>>;

public class GetProductsHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetProducts, ApiResponse<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetProducts request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _context.Products.
                Include(x => x.Manufacturer).
                Include(x => x.ProductCategories).
                Include(x => x.Franchise).
                ThenInclude(x => x.Series).
                AsNoTracking()
                .Select(x => new ProductDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ManufacturerName = x.Manufacturer.ManufacturerName,
                    FranchiseName = x.Franchise.FranchiseName,
                    SerieName = x.Serie != null ? x.Serie.SerieName : "Toda la Franquicia",
                    Categories = x.ProductCategories.Select(cp => cp.Category.CategoryName).ToList(),
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);

            return _responseService.Success(products);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<ProductDto>>("An error occurred while processing your request");
        }
    }
}