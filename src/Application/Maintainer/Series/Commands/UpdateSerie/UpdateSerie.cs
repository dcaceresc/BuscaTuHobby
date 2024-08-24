namespace Application.Maintainer.Series.Commands.UpdateSerie;

public record UpdateSerie(Guid SerieId, string SerieName, Guid FranchiseId) : IRequest<ApiResponse>;
public class UpdateSerieHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateSerie, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateSerie request, CancellationToken cancellationToken)
    {
        try
        {
            var serie = await _context.Series.FindAsync([request.SerieId], cancellationToken);

            Guard.Against.NotFound(serie, $"No existe serie con Id {request.SerieId}");

            serie.Update(request.SerieName, request.FranchiseId);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Serie actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la serie");
        }
    }
}