namespace Application.Maintainer.Groups.Queries.GetGroupById;

public record GetGroupById(Guid GroupId) : IRequest<ApiResponse<GroupVM>>;

public class GetCategoryByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetGroupById, ApiResponse<GroupVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<GroupVM>> Handle(GetGroupById request, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _context.Groups.
            ProjectTo<GroupVM>(_mapper.ConfigurationProvider).
            FirstOrDefaultAsync(x => x.GroupId == request.GroupId, cancellationToken);

            Guard.Against.NotFound(group, $"No existe grupo con la Id {request.GroupId}");

            return _responseService.Success(group);
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail<GroupVM>(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<GroupVM>("No se pudo obtener el grupo");
        }
    }
}

