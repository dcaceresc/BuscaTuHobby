using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;


namespace Application.Grades.Commands.DeleteGrade;

public class ToggleGradeCommand : IRequest
{
    public int id { get; set; }

    public class ToggleGradeCommandHandler : IRequestHandler<ToggleGradeCommand>
    {
        private readonly IApplicationDbContext _context;
        public ToggleGradeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleGradeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Grades.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Grade), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}