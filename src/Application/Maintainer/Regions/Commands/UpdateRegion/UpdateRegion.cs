namespace Application.Maintainer.Regions.Commands.UpdateRegion;
public record UpdateRegion(Guid RegionId, string RegionName) : IRequest<ApiResponse>;

public class UpdateRegionHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateRegion, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateRegion request, CancellationToken cancellationToken)
    {
        try
        {
            var region = await _context.Regions.FindAsync([request.RegionId], cancellationToken);

            Guard.Against.NotFound(region, $"No existe la región con la Id {request.RegionId}");

            region.Update(request.RegionName);

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
