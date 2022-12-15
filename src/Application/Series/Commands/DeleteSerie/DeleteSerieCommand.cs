using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Series.Commands.DeleteSerie;

public class DeleteSerieCommand : IRequest
{
    public int id { get; set; }

    public class DeleteSerieCommandHandler : IRequestHandler<DeleteSerieCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteSerieCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteSerieCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Series.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Serie), request.id);
            }

            _context.Series.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

