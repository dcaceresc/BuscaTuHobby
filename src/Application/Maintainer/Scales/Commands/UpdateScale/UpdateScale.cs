namespace Application.Maintainer.Scales.Commands.UpdateScale;

public record UpdateScale(Guid ScaleId, string ScaleName) : IRequest<ApiResponse>;

public class UpdateScaleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateScale, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateScale request, CancellationToken cancellationToken)
    {
        try
        {
            var scale = await _context.Scales.FindAsync([request.ScaleId], cancellationToken);

            Guard.Against.NotFound(scale, $"No existe una escala con la Id {request.ScaleId}");

            scale.Update(request.ScaleName);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Escala actualizada correctamente");
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