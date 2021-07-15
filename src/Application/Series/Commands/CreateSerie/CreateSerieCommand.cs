using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Series.Commands.CreateSerie
{
    public class CreateSerieCommand : IRequest<int>
    {
        public string Name { get; set; }
        public int UniverseId { get; set; }

        public class CreateSerieCommandHandler : IRequestHandler<CreateSerieCommand, int>
        {
            private readonly IApplicationDbContext _context;

            public CreateSerieCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<int> Handle(CreateSerieCommand request, CancellationToken cancellationToken)
            {
                var entity = new Serie
                {
                    Name = request.Name,
                    UniverseId = request.UniverseId
                };

                _context.Series.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
        }
    }
}
