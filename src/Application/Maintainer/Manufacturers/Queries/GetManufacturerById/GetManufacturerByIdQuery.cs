namespace Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
public record GetManufacturerByIdQuery(Guid ManufacturerId) : IRequest<ManufacturerVM>;

public class GetManufacturerByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetManufacturerByIdQuery, ManufacturerVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<ManufacturerVM> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
    {
        var manufacturer = await _context.Manufacturers
            .ProjectTo<ManufacturerVM>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.ManufacturerId == request.ManufacturerId, cancellationToken);

        Guard.Against.NotFound(request.ManufacturerId, manufacturer);

        return manufacturer;
    }
}