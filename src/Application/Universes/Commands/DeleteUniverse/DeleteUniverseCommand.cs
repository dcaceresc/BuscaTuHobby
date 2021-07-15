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

namespace Application.Universes.Commands.DeleteUniverse
{
    public class DeleteUniverseCommand : IRequest
    {
        public int Id { get; set; }

        public class DeleteUniverseCommandHandler : IRequestHandler<DeleteUniverseCommand>
        {
            private readonly IApplicationDbContext _context;
            public DeleteUniverseCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteUniverseCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Universes.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Universe), request.Id);
                }

                _context.Universes.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}

