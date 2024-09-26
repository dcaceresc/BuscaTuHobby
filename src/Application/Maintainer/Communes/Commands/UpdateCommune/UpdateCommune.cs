namespace Application.Maintainer.Communes.Commands.UpdateCommune;
public record UpdateCommune(Guid CommuneId, string CommuneName, Guid RegionId) : IRequest<ApiResponse>;

public class UpdateCommuneHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateCommune, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateCommune request, CancellationToken cancellationToken)
    {
        try
        {
            var commune = await _context.Communes.FindAsync([request.CommuneId], cancellationToken);

            Guard.Against.NotFound(commune, $"No existe ciudad con la Id {request.CommuneId}");

            commune.Update(request.CommuneName, request.RegionId);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido actualizada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al actualizar la cuidad");
        }
    }
}
