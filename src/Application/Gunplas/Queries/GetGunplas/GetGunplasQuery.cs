namespace Application.Gunplas.Queries.GetGunplas;

public class GetGunplasQuery : IRequest<IList<GunplaDto>>
{
    public class GetGunplasQueryHandler : IRequestHandler<GetGunplasQuery, IList<GunplaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetGunplasQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<GunplaDto>> Handle(GetGunplasQuery request, CancellationToken cancellationToken)
        {
            return await _context.Gunplas.AsNoTracking().ProjectTo<GunplaDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        }
    }
}