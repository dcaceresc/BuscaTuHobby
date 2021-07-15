using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Scales.Commands.CreateScale
{
    public class CreateScaleCommand : IRequest<int>
    {
        public string Name { get; set; }

        public class CreateScaleCommandHandler : IRequestHandler<CreateScaleCommand, int>
        {
            private readonly IApplicationDbContext _context;

            public CreateScaleCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }


            public async Task<int> Handle(CreateScaleCommand request, CancellationToken cancellationToken)
            {
                var entity = new Scale
                {
                    Name = request.Name
                };

                _context.Scales.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
        }
    }
}
