namespace Application.Maintainer.Manufacturers.Queries.GetManufacturers;

public record GetManufacturers : IRequest<ApiResponse<List<ManufacturerDto>>>;
public class GetManufacturersHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetManufacturers, ApiResponse<List<ManufacturerDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<ManufacturerDto>>> Handle(GetManufacturers request, CancellationToken cancellationToken)
    {
        try
        {
            var manufacturers = await _context.Manufacturers
            .AsNoTracking()
            .Select(x => new ManufacturerDto
            {
                ManufacturerId = x.ManufacturerId,
                ManufacturerName = x.ManufacturerName,
                IsActive = x.IsActive,
            })
            .ToListAsync(cancellationToken);

            return _responseService.Success(manufacturers);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<ManufacturerDto>>("No se pudo obtener los fabricantes");
        }
    }
}