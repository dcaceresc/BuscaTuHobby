using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Grades.Commands.UpdateGrade;

public class UpdateGradeCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;
}

public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateGradeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Grades.FindAsync(request.id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Grade), request.id);
        }

        entity.name = request.name;
        entity.acronym = request.acronym;

        await _context.SaveChangesAsync(cancellationToken);
    }
}



