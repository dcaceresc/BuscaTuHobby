using Application.Maintainer.Cities.Queries.GetCommunes;

namespace Application.Maintainer.Communes.Queries.GetCommunes;
public record GetCommunes : IRequest<ApiResponse<List<CommuneDto>>>;

public class GetCommunesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCommunes, ApiResponse<List<CommuneDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CommuneDto>>> Handle(GetCommunes request, CancellationToken cancellationToken)
    {
        try
        {
            var comunnes = await _context.Communes
                .Include(x => x.Region)
                .ProjectTo<CommuneDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);


            return _responseService.Success(comunnes);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<CommuneDto>>("Ah ocurrido un error al obtener las ciudades");
        }
    }
}