using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Series.Commands.ToggleSerie;

public class ToggleSerieCommand : IRequest
{
    public int id { get; set; }

    public class ToggleSerieCommandHandler : IRequestHandler<ToggleSerieCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleSerieCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleSerieCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Series.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Serie), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

