using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Grades.Commands.DeleteGrade
{
    public class DeleteGradeCommand : IRequest
    {
        public int Id { get; set; }

        public class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand>
        {
            private readonly IApplicationDbContext _context;
            public DeleteGradeCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteGradeCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Grades.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Grade), request.Id);
                }

                _context.Grades.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
