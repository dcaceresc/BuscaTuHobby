namespace Application.Maintainer.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryVM>;

public class GetSubCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, CategoryVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<CategoryVM> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(x => x.CategoryId == request.CategoryId)
            .AsNoTracking()
            .ProjectTo<CategoryVM>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);
    }
}
