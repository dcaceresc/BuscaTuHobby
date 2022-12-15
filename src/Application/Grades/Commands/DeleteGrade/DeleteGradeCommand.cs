using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;


namespace Application.Grades.Commands.DeleteGrade;

public class DeleteGradeCommand : IRequest
{
    public int id { get; set; }

    public class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand>
    {
        private readonly IApplicationDbContext _context;
        public DeleteGradeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteGradeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Grades.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Grade), request.id);
            }

            _context.Grades.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}