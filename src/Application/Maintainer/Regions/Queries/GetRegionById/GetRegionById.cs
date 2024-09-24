namespace Application.Maintainer.Regions.Queries.GetRegionById;
public record GetRegionById(Guid RegionId) : IRequest<ApiResponse<RegionVM>>;

public class GetRegionByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetRegionById, ApiResponse<RegionVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<RegionVM>> Handle(GetRegionById request, CancellationToken cancellationToken)
    {
        try
        {
            var region = await _context.Regions
            .AsNoTracking()
            .ProjectTo<RegionVM>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.RegionId == request.RegionId, cancellationToken);

            Guard.Against.NotFound(region, $"No existe la región con la Id {request.RegionId}");

            return _responseService.Success(region);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<RegionVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<RegionVM>("No se pudo obtener la región");
        }
    }
}
