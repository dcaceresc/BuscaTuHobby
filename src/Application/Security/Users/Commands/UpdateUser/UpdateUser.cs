namespace Application.Security.Users.Commands.UpdateUser;
public record UpdateUser : IRequest<ApiResponse>
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public bool EmailConfirmed { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTime? LockoutEnd { get; init; }
    public IList<Guid> RoleIds { get; init; } = default!;

}

public class UpdateUserHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateUser, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateUser request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

            Guard.Against.NotFound(user, $"No existe usuario con la Id {request.UserId}");

            user.Update(request.Email, request.EmailConfirmed, request.LockoutEnabled, request.LockoutEnd);

            _context.Users.Update(user);

            await _context.SaveChangesAsync(cancellationToken);

            var oldUserRoles = await _context.UserRoles.Where(x => x.UserId == request.UserId).ToListAsync(cancellationToken);

            foreach (var role in request.RoleIds)
            {
                var existingUserRole = user.UserRoles.FirstOrDefault(x => x.RoleId == role);

                if (existingUserRole == null)
                {
                    var userRole = user.AssignRole(role);

                    _context.UserRoles.Add(userRole);
                }
                else
                {
                    existingUserRole.LastModified = DateTime.Now;
                }
            }

            foreach (var userRole in oldUserRoles)
            {
                if (!request.RoleIds.Contains(userRole.RoleId))
                {
                    _context.UserRoles.Remove(userRole);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);


            return _responseService.Success("Usuario actualizado exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el usuario");
        }
    }
}
