namespace Application.Maintainer.Manufacturers.Commands.ToggleManufacturer;

public record ToggleManufacturerCommand(Guid ManufacturerId) : IRequest;

public class ToggleManufacturerCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleManufacturerCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleManufacturerCommand request, CancellationToken cancellationToken)
    {
        var manufacturer = await _context.Manufacturers.FindAsync([request.ManufacturerId], cancellationToken);

        Guard.Against.NotFound(request.ManufacturerId, manufacturer);

        manufacturer.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}
