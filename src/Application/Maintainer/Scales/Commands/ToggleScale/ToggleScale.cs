namespace Application.Maintainer.Scales.Commands.ToggleScale;

public record ToggleScale(Guid ScaleId) : IRequest<ApiResponse>;

public class ToggleScaleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleScale, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleScale request, CancellationToken cancellationToken)
    {
        try
        {
            var scale = await _context.Scales.FindAsync([request.ScaleId], cancellationToken);

            Guard.Against.NotFound(scale, $"No existe escala con la Id {request.ScaleId}");

            scale.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la escala actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la escala");
        }
    }
}