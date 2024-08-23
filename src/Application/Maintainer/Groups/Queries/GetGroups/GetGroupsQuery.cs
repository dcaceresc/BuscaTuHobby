namespace Application.Maintainer.Groups.Queries.GetGroups;

public class GetGroups : IRequest<ApiResponse<List<GroupDto>>>;

public class GetGroupsHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetGroups, ApiResponse<List<GroupDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<GroupDto>>> Handle(GetGroups request, CancellationToken cancellationToken)
    {
        try
        {
            var groups = await _context.Groups
            .AsNoTracking()
            .ProjectTo<GroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(groups);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<GroupDto>>("No se pudo obtener los grupos");
        }
    }
}