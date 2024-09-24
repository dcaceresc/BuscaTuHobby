
namespace Application.Maintainer.Cities.Queries.GetCityById;
public record GetCityById(Guid CityId) : IRequest<ApiResponse<CityVM>>;

public class GetCityByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCityById, ApiResponse<CityVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<CityVM>> Handle(GetCityById request, CancellationToken cancellationToken)
    {
		try
		{
            var city = await _context.Cities
                .Include(x => x.Region)
                .ProjectTo<CityVM>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.CityId == request.CityId,cancellationToken);

            Guard.Against.NotFound(city, $"No existe cuidad con la Id {request.CityId}");

            return _responseService.Success(city);
		}
        catch(Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<CityVM>(ex.Message);
        }
		catch (Exception)
		{
            return _responseService.Fail<CityVM>("Ah ocurrido un error al obtener la ciudad");
		}
    }
}
