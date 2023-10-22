namespace Application.Maintainer.Groups.Queries.GetGroupById;

public class GetGroupByIdQuery : IRequest<GroupVM>
{
    public int id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, GroupVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GroupVM> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Groups.Where(x => x.id == request.id).AsNoTracking().ProjectTo<GroupVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}

