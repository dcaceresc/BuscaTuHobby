using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Gunplas.Commands.ToggleGunpla;

public class ToggleGunplaCommand : IRequest
{
    public int id { get; set; }

    public class ToggleGunplaCommandHandler : IRequestHandler<ToggleGunplaCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleGunplaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleGunplaCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Gunplas.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Gunpla), request.id);

            entity.actve = !entity.actve;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
