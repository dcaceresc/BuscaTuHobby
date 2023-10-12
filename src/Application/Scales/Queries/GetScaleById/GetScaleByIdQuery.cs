namespace Application.Scales.Queries.GetScaleById;

public class GetScaleByIdQuery : IRequest<ScaleVM>
{

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

    public Task<ScaleVM> Handle(GetScaleByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}