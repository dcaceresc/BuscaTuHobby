namespace Application.Security.Users.Queries.GetUsers;
public record GetUsers : IRequest<ApiResponse<List<UserDto>>>;

public class GetUsersHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetUsers, ApiResponse<List<UserDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<UserDto>>> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _context.Users
            .Select(x => new UserDto
            {
                UserId = x.UserId,
                Email = x.Email,
                EmailConfirmed = x.EmailConfirmed,
                LockoutEnabled = x.LockoutEnabled,
                LockoutEnd = x.LockoutEnd.HasValue ? x.LockoutEnd.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty,
                RoleNames = x.UserRoles.Select(r => r.Role.RoleName).ToList(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

            return _responseService.Success(users);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<UserDto>>("Error al obtener los usuarios");
        }
    }
}