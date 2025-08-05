namespace Application.Security.Users.Queries.GetUserById;
public record GetUserById(Guid UserId) : IRequest<ApiResponse<UserVM>>;

public class GetUserByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetUserById, ApiResponse<UserVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<UserVM>> Handle(GetUserById request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Select(x =>  new UserVM
                {
                    UserId = x.UserId,
                    Email = x.Email,
                    EmailConfirmed = x.EmailConfirmed,
                    LockoutEnabled = x.LockoutEnabled,
                    LockoutEnd = x.LockoutEnd,
                    RoleIds = x.UserRoles.Select(r => r.RoleId).ToList()
                })
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);


            Guard.Against.NotFound(user, $"No existe usuario con la Id {request.UserId}");

            return _responseService.Success(user);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<UserVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<UserVM>("Error al obtener el usuario.");
        }
    }
}
