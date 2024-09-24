namespace Application.Maintainer.Cities.Queries.GetCities;
public record GetCities : IRequest<ApiResponse<List<CityDto>>>;

public class GetCitiesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCities, ApiResponse<List<CityDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CityDto>>> Handle(GetCities request, CancellationToken cancellationToken)
    {
		try
		{
            var cities = await _context.Cities
                .Include(x => x.Region)
                .ProjectTo<CityDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);


            return _responseService.Success(cities);
        }
		catch (Exception)
		{
            return _responseService.Fail<List<CityDto>>("Ah ocurrido un error al obtener las ciudades");
        }
    }
}