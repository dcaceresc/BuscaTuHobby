namespace Application.Maintainer.Groups.Commands.ToggleGroup;

public record ToggleGroup(Guid GroupId) : IRequest<ApiResponse>;

public class ToggleGroupHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleGroup, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleGroup request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe grupo con la id {request.GroupId}");

            entity.ToggleActive();

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