namespace Application.Maintainer.Regions.Commands.CreateRegion;
public record CreateRegion(string RegionName, int RegionOrder) : IRequest<ApiResponse>;

public class CreateRegionHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateRegion, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateRegion request, CancellationToken cancellationToken)
    {
        try
        {
            var region = Region.Create(request.RegionName,request.RegionOrder);

            _context.Regions.Add(region);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Región creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear la región");
        }
    }
}
