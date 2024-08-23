namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public record UpdateGroup(Guid GroupId, string GroupName) : IRequest<ApiResponse>;

public class UpdateGroupHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateGroup, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateGroup request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe grupo con la Id {request.GroupId}");

            entity.Update(request.GroupName);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Grupo actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el grupo");
        }
    }
}



