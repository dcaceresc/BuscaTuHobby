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

namespace Application.Grades.Commands.UpdateGrade
{
    public class UpdateGradeCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }

        public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand>
        {
            private readonly IApplicationDbContext _context;
            public UpdateGradeCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Unit> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Grades.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Grade), request.Id);
                }

                entity.Name = request.Name;
                entity.Acronym = request.Acronym;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }

    }

    
}
