namespace Application.Maintainer.Products.Queries.GetProducts;

public record GetProducts : IRequest<ApiResponse<List<ProductDto>>>;

public class GetProductsHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetProducts, ApiResponse<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetProducts request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _context.Products.
                Include(x => x.Scale).
                Include(x => x.Manufacturer).
                Include(x => x.ProductCategories).
                Include(x => x.Franchise).
                ThenInclude(x => x.Series).
                AsNoTracking().
                ProjectTo<ProductDto>(_mapper.ConfigurationProvider).
                ToListAsync(cancellationToken);

            return _responseService.Success(products);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<ProductDto>>("An error occurred while processing your request");
        }
    }
}