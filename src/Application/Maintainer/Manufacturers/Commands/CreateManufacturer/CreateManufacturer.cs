namespace Application.Maintainer.Manufacturers.Commands.CreateManufacturer;

public record CreateManufacturer(string ManufacturerName) : IRequest<ApiResponse>;
public class CreateManufacturerHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateManufacturer, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateManufacturer request, CancellationToken cancellationToken)
    {
        try
        {
            var manufacturer = Manufacturer.Create(request.ManufacturerName);

            _context.Manufacturers.Add(manufacturer);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Fabricante creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el fabricante");
        }
    }
}
