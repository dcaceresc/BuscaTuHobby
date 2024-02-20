namespace Application.Maintainer.Categories.Queries.GetCategories;
public record GetCategoriesQuery : IRequest<IList<CategoryDto>>;

public class GetSubCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetCategoriesQuery, IList<CategoryDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Include(x => x.Group)
            .AsNoTracking()
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}


