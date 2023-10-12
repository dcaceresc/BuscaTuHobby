namespace Application.Grades.Queries.GetGradeById;

public class GetGradesByIdQuery : IRequest<GradeVM>
{
    public int id { get; set; }
}

public class GetGradesByIdQueryHandler : IRequestHandler<GetGradesByIdQuery, GradeVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGradesByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GradeVM> Handle(GetGradesByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Grades.Where(x => x.id == request.id).AsNoTracking().ProjectTo<GradeVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}

