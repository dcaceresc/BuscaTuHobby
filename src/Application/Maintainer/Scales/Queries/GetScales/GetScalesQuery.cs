namespace Application.Maintainer.Scales.Queries.GetScales;

public record GetScalesQuery : IRequest<IList<ScaleDto>>;

public class GetScalesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetScalesQuery, IList<ScaleDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<ScaleDto>> Handle(GetScalesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Scales
            .AsNoTracking()
            .ProjectTo<ScaleDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}

