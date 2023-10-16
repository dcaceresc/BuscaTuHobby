namespace Application.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQuery : IRequest<CategoryVM>
{
    public int id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CategoryVM> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories.Where(x => x.id == request.id).AsNoTracking().ProjectTo<CategoryVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}

