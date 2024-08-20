namespace Application.Maintainer.Manufacturers.Commands.CreateManufacturer;

public record CreateManufacturerCommand(string ManufacturerName) : IRequest<Guid>;
public class CreateManufacturerCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateManufacturerCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
    {
        var manufacturer = Manufacturer.Create(request.ManufacturerName);

        _context.Manufacturers.Add(manufacturer);

        await _context.SaveChangesAsync(cancellationToken);

        return manufacturer.ManufacturerId;
    }
}
