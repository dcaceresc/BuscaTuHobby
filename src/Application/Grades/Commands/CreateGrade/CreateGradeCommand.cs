using Domain.Entities;

namespace Application.Grades.Commands.CreateGrade;
public class CreateGradeCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;


    public class CreateGradeCommandHandler : IRequestHandler<CreateGradeCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateGradeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
        {
            var entity = new Grade
            {
                name = request.name,
                acronym = request.acronym
            };

            _context.Grades.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}