using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Grades.Commands.CreateGrade
{
    public class CreateGradeCommand : IRequest<int>
    {
        public string Name { get; set; }
        public string Acronym { get; set; }


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
                    Name = request.Name,
                    Acronym = request.Acronym
                };

                _context.Grades.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return entity.Id;
            }
        }
    }
}
