namespace Application.Maintainer.Manufacturers.Commands.ToggleManufacturer;

public record ToggleManufacturer(Guid ManufacturerId) : IRequest<ApiResponse>;

public class ToggleManufacturerHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleManufacturer, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleManufacturer request, CancellationToken cancellationToken)
    {
        try
        {
            var manufacturer = await _context.Manufacturers.FindAsync([request.ManufacturerId], cancellationToken);

            Guard.Against.NotFound(manufacturer,$"No existe fabricante con la Id {request.ManufacturerId}");

            manufacturer.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Fabricante actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el fabricante");
        }
    }
}
