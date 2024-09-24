namespace Application.Maintainer.Cities.Commands.ToggleCity;
public record ToggleCity(Guid CityId) : IRequest<ApiResponse>;

public class ToggleCityHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleCity, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleCity request, CancellationToken cancellationToken)
    {
        try
        {
            var city = await _context.Cities.FindAsync([request.CityId],cancellationToken);

            Guard.Against.NotFound(city, $"No existe ciudad con la Id {request.CityId}");

            city.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido actualizada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al actualizar la cuidad");
        }
    }
}
