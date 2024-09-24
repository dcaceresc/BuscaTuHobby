namespace Application.Maintainer.Regions.Commands.ToggleRegion;
public record ToggleRegion(Guid RegionId) : IRequest<ApiResponse>;

public class ToggleRegionHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleRegion, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleRegion request, CancellationToken cancellationToken)
    {
        try
        {
            var region = await _context.Regions.FindAsync([request.RegionId],cancellationToken);

            Guard.Against.NotFound(region, $"No existe la región con la Id {request.RegionId}");

            region.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Región actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar la región");
        }
    }
}
