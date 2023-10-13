namespace Application.Scales.Queries.GetScaleById;

public class GetScaleByIdQuery : IRequest<ScaleVM>
{
    public int id { get; set; }
}

public class GetScaleByIdQueryHandler : IRequestHandler<GetScaleByIdQuery, ScaleVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetScaleByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ScaleVM> Handle(GetScaleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Scales.Where(x => x.id == request.id).AsNoTracking().ProjectTo<ScaleVM>(_mapper.ConfigurationProvider).FirstAsync();
    }
}