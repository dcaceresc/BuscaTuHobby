namespace Application.Grades.Queries.GetGrades;

public class GetGradesQuery : IRequest<IList<GradeDto>>
{
    public class GetGradesQueryHandler : IRequestHandler<GetGradesQuery, IList<GradeDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetGradesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<GradeDto>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Grades.AsNoTracking().ProjectTo<GradeDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

