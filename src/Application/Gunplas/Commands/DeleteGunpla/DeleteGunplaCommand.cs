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

namespace Application.Gunplas.Commands.DeleteGunpla
{
    public class DeleteGunplaCommand : IRequest
    {
        public int Id { get; set; }

        public class DeleteGunplaCommandHandler : IRequestHandler<DeleteGunplaCommand>
        {
            private readonly IApplicationDbContext _context;

            public DeleteGunplaCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(DeleteGunplaCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Gunplas.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Gunpla), request.Id);
                }

                _context.Gunplas.Remove(entity);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
