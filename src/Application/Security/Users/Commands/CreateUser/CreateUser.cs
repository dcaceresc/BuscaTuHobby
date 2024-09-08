namespace Application.Security.Users.Commands.CreateUser;
public record CreateUser : IRequest<ApiResponse>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;

}

public class CreateUserHandler(IApplicationDbContext context, IApiResponseService responseService, IIdentityService identityService) : IRequestHandler<CreateUser, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IIdentityService _identityService = identityService;

    public async Task<ApiResponse> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = User.Create(request.Email, _identityService.HashPassword(request.Password));

            _context.Users.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Usuario creado exitosamente");

        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear el usuario");
        }
    }
}

