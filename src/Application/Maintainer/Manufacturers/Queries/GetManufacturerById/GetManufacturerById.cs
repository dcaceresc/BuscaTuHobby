namespace Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
public record GetManufacturerById(Guid ManufacturerId) : IRequest<ApiResponse<ManufacturerVM>>;

public class GetManufacturerByIdHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetManufacturerById, ApiResponse<ManufacturerVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<ManufacturerVM>> Handle(GetManufacturerById request, CancellationToken cancellationToken)
    {
        try
        {
            var manufacturer = await _context.Manufacturers
           .ProjectTo<ManufacturerVM>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(x => x.ManufacturerId == request.ManufacturerId, cancellationToken);

            Guard.Against.NotFound(manufacturer, $"No existe fabricante con la Id {request.ManufacturerId}");

            return _responseService.Success(manufacturer);
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail<ManufacturerVM>(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<ManufacturerVM>("No se pudo obtener el fabricante");
        }

       
    }
}