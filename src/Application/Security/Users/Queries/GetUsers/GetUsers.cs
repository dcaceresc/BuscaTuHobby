namespace Application.Security.Users.Queries.GetUsers;
public record GetUsers : IRequest<ApiResponse<List<UserDto>>>;

public class GetUsersHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetUsers, ApiResponse<List<UserDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<UserDto>>> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _context.Users
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(users);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<UserDto>>("Error al obtener los usuarios");
        }
    }
}