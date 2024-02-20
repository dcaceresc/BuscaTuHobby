namespace Application.Maintainer.Manufacturers.Queries.GetManufacturers;

public record GetManufacturersQuery : IRequest<IList<ManufacturerDto>>;
public class GetManufacturersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetManufacturersQuery, IList<ManufacturerDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<ManufacturerDto>> Handle(GetManufacturersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Manufacturers
            .AsNoTracking()
            .ProjectTo<ManufacturerDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}