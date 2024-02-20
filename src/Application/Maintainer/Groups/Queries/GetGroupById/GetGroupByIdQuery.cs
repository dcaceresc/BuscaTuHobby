namespace Application.Maintainer.Groups.Queries.GetGroupById;

public record GetGroupByIdQuery(Guid GroupId) : IRequest<GroupVM>;

public class GetCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetGroupByIdQuery, GroupVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<GroupVM> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Groups.
            ProjectTo<GroupVM>(_mapper.ConfigurationProvider).
            FirstOrDefaultAsync(x => x.GroupId == request.GroupId, cancellationToken);

        Guard.Against.NotFound(request.GroupId, category);

        return category;
    }
}

