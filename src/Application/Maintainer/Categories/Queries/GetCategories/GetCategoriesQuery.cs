namespace Application.Maintainer.Categories.Queries.GetCategories;
public class GetCategoriesQuery : IRequest<IList<CategoryDto>>
{

}

public class GetSubCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IList<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSubCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<IList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories.Include(x => x.Group).AsNoTracking().ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}


