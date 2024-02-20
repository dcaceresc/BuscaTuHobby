namespace Application.Maintainer.Groups.Queries.GetGroups;

public class GetGroupsQuery : IRequest<IList<GroupDto>>;

public class GetGradesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetGroupsQuery, IList<GroupDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<GroupDto>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Groups
            .AsNoTracking()
            .ProjectTo<GroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}