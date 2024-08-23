namespace Application.Maintainer.Franchises.Commands.ToggleFranchise;
public record ToggleFranchise(Guid FranchiseId) : IRequest<ApiResponse>;

public class ToggleFranchiseHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleFranchise,ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleFranchise request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Franchises.FindAsync([request.FranchiseId], cancellationToken);

            Guard.Against.NotFound(entity,$"No existe franquicia con Id {request.FranchiseId}");

            entity.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la franquicia ha sido actualizado exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el estado de la franquicia");
        }

        
    }
}