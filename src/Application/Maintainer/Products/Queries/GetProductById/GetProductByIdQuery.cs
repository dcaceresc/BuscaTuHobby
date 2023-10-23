namespace Application.Maintainer.Products.Queries.GetProductById;
public class GetProductByIdQuery : IRequest<ProductVM>
{
    public int id { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IApplicationDbContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<ProductVM> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products.
               Include(x => x.Scale).
               Include(x => x.Manufacturer).
               Include(x => x.CategoryProducts).
               Include(x => x.Franchise).
               ThenInclude(x => x.Serie).
               Where(x => x.id == request.id).
               AsNoTracking().
               ProjectTo<ProductVM>(_mapper.ConfigurationProvider).
               FirstAsync(cancellationToken);
    }
}
