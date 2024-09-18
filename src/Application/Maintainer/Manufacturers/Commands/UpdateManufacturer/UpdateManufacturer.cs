namespace Application.Maintainer.Manufacturers.Commands.UpdateManufacturer;

public record UpdateManufacturer(Guid ManufacturerId, string ManufacturerName) : IRequest<ApiResponse>;

public class UpdateManufacturerHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateManufacturer, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<ApiResponse> Handle(UpdateManufacturer request, CancellationToken cancellationToken)
    {
        try
        {
            var manufacturer = await _context.Manufacturers.FindAsync([request.ManufacturerId], cancellationToken);

            Guard.Against.NotFound(manufacturer, $"No existe fabricante con la Id {request.ManufacturerId}");

            manufacturer.Update(request.ManufacturerName);

            await _context.SaveChangesAsync(cancellationToken);

            return responseService.Success("Fabricante actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return responseService.Fail("No se pudo actualizar el fabricante");
        }
    }
}