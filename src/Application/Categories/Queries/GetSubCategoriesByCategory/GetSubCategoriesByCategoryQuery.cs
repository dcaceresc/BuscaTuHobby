namespace Application.Categories.Queries.GetSubCategoriesByCategory;

public class GetSubCategoriesByCategoryQuery : IRequest<IList<SubCategoryDto>>
{
    public int categoryId { get; set; }
}

public class GetSubCategoriesQueryHandler : IRequestHandler<GetSubCategoriesByCategoryQuery, IList<SubCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSubCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IList<SubCategoryDto>> Handle(GetSubCategoriesByCategoryQuery request, CancellationToken cancellationToken)
    {
        return await _context.SubCategories.Where(x => x.categoryId == request.categoryId).AsNoTracking().ProjectTo<SubCategoryDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}