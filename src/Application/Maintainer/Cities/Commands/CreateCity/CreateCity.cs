namespace Application.Maintainer.Cities.Commands.CreateCity;
public record CreateCity(string CityName, Guid RegionId) : IRequest<ApiResponse>;

public class CreateCityHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCity, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCity request, CancellationToken cancellationToken)
    {
        try
        {
            var city = City.Create(request.CityName, request.RegionId);

            _context.Cities.Add(city);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("La cuidad ha sido creada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ah ocurrido un error al crear la ciudad");
        }
    }
}
