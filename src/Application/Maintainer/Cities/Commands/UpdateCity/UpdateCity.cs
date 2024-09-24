namespace Application.Maintainer.Cities.Commands.UpdateCity;
public record UpdateCity(Guid CityId, string CityName, Guid RegionId) : IRequest<ApiResponse>;

public class UpdateCityHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateCity, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateCity request, CancellationToken cancellationToken)
    {
        try
        {
            var city = await _context.Cities.FindAsync([request.CityId], cancellationToken);

            Guard.Against.NotFound(city, $"No existe ciudad con la Id {request.CityId}");

            city.Update(request.CityName, request.RegionId);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido actualizada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al actualizar la cuidad");
        }
    }
}
