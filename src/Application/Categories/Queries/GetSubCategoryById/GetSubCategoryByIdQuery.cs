namespace Application.Categories.Queries.GetSubCategoryById;

public class GetSubCategoryByIdQuery : IRequest<SubCategoryVM>
{
    public int id { get; set; }
}

public class GetSubCategoryByIdQueryHandler : IRequestHandler<GetSubCategoryByIdQuery, SubCategoryVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSubCategoryByIdQueryHandler(IApplicationDbContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SubCategoryVM> Handle(GetSubCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.SubCategories.Where(x => x.id == request.id).AsNoTracking().ProjectTo<SubCategoryVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}
