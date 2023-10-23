using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Series.Commands.UpdateSerie;

public class UpdateSerieCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int franchiseId { get; set; }
    public class UpdateSerieCommandHandler : IRequestHandler<UpdateSerieCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSerieCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateSerieCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Series.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Serie), request.id);

            entity.name = request.name;
            entity.franchiseId = request.franchiseId;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}

