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

namespace Application.Series.Commands.DeleteSerie
{
    public class DeleteSerieCommand : IRequest
    {
        public int Id { get; set; }

        public class DeleteSerieCommandHandler : IRequestHandler<DeleteSerieCommand>
        {
            private readonly IApplicationDbContext _context;

            public DeleteSerieCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteSerieCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Series.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Serie), request.Id);
                }

                _context.Series.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
