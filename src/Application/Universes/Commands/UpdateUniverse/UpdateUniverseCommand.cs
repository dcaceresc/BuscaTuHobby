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

namespace Application.Universes.Commands.UpdateUniverse
{
    public class UpdateUniverseCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public class UpdateUniverseCommandHandler : IRequestHandler<UpdateUniverseCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateUniverseCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateUniverseCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Universes.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Universe), request.Id);
                }

                entity.Name = request.Name;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
