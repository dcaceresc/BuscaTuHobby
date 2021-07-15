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

namespace Application.Series.Commands.UpdateSerie
{
    public class UpdateSerieCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UniverseId { get; set; }

        public class UpdateSerieCommandHandler : IRequestHandler<UpdateSerieCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateSerieCommandHandler(IApplicationDbContext context )
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateSerieCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Series.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Serie), request.Id);
                }

                entity.Name = request.Name;
                entity.UniverseId = request.UniverseId;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
