namespace Application.Maintainer.Communes.Queries.GetCommuneById;
public record GetCommuneById(Guid CommuneId) : IRequest<ApiResponse<CommuneVM>>;

public class GetCommuneByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCommuneById, ApiResponse<CommuneVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<CommuneVM>> Handle(GetCommuneById request, CancellationToken cancellationToken)
    {
        try
        {
            var commune = await _context.Communes
                .Include(x => x.Region)
                .ProjectTo<CommuneVM>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.CommuneId == request.CommuneId, cancellationToken);

            Guard.Against.NotFound(commune, $"No existe cuidad con la Id {request.CommuneId}");

            return _responseService.Success(commune);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<CommuneVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<CommuneVM>("Ah ocurrido un error al obtener la ciudad");
        }
    }
}
