namespace Application.Manufacturers.Queries.GetManufacturerById;
public class GetManufacturerByIdQuery : IRequest<ManufacturerVM>
{
    public int id { get; set; }
}

public class GetManufacturerByIdQueryHandler : IRequestHandler<GetManufacturerByIdQuery, ManufacturerVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetManufacturerByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<ManufacturerVM> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Manufacturers.Where(x => x.id == request.id).AsNoTracking().ProjectTo<ManufacturerVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}