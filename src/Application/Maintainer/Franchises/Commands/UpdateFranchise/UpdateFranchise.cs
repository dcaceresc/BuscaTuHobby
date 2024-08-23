namespace Application.Maintainer.Franchises.Commands.UpdateFranchise;
public record UpdateFranchise(Guid FranchiseId, string FranchiseName) : IRequest<ApiResponse>;

public class UpdateFranchiseHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateFranchise,ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateFranchise request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Franchises.FindAsync([request.FranchiseId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe franquicia con Id {request.FranchiseId}");

            entity.Update(request.FranchiseName);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Franquicia actualizada exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar la franquicia");
        }
    }
}
