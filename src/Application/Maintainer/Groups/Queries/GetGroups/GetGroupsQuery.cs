namespace Application.Maintainer.Groups.Queries.GetGroups;

public class GetGroupsQuery : IRequest<IList<GroupDto>>
{
    public class GetGradesQueryHandler : IRequestHandler<GetGroupsQuery, IList<GroupDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetGradesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<GroupDto>> Handle(GetGroupsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Groups.AsNoTracking().ProjectTo<GroupDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

