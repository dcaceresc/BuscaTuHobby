namespace Application.Maintainer.Scales.Queries.GetScaleById;

public record GetScaleByIdQuery(Guid ScaleId) : IRequest<ScaleVM>;

public class GetScaleByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetScaleByIdQuery, ScaleVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<ScaleVM> Handle(GetScaleByIdQuery request, CancellationToken cancellationToken)
    {
        var scale = await _context.Scales
               .AsNoTracking()
               .ProjectTo<ScaleVM>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync(x => x.ScaleId == request.ScaleId, cancellationToken);

        Guard.Against.NotFound(request.ScaleId, scale);

        return scale;
    }
}