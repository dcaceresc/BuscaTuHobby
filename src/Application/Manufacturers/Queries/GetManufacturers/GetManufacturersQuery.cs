namespace Application.Manufacturers.Queries.GetManufacturers;

public class GetManufacturersQuery : IRequest<IList<ManufacturerDto>>
{
    public class GetManufacturersQueryHandler : IRequestHandler<GetManufacturersQuery, IList<ManufacturerDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetManufacturersQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IList<ManufacturerDto>> Handle(GetManufacturersQuery request, CancellationToken cancellationToken)
        {
            return await _context.Manufacturers.AsNoTracking().ProjectTo<ManufacturerDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}