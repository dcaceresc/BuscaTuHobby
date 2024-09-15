namespace Application.Security.Users.Commands.ToggleUser;
public record ToggleUser(Guid UserId) : IRequest<ApiResponse>;

public class TogleUserHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleUser, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleUser request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

            Guard.Against.NotFound(user, $"No existe usuario con la Id {request.UserId}");

            user.ToggleActive();

            _context.Users.Update(user);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Usuario actualizado exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el usuario");
        }
    }
}


