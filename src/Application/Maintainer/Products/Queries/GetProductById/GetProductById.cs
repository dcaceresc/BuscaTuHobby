namespace Application.Maintainer.Products.Queries.GetProductById;
public record GetProductById(Guid ProductId) : IRequest<ApiResponse<ProductVM>>;

public class GetProductByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetProductById, ApiResponse<ProductVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ProductVM>> Handle(GetProductById request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products
                           .Include(x => x.Scale)
                           .Include(x => x.Manufacturer)
                           .Include(x => x.ProductCategories)
                           .Include(x => x.Franchise)
                           .ThenInclude(x => x.Series)
                           .AsNoTracking()
                           .ProjectTo<ProductVM>(_mapper.ConfigurationProvider)
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
