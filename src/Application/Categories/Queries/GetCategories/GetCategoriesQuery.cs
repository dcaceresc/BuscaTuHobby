namespace Application.Categories.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<IList<CategoryDto>>
{
    public class GetGradesQueryHandler : IRequestHandler<GetCategoriesQuery, IList<CategoryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetGradesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Categories.AsNoTracking().ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

