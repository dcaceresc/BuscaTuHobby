using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Scales.Commands.ToggleScale;

public class ToggleScaleCommand : IRequest
{
    public int id { get; set; }

    public class ToggleScaleCommandHandler : IRequestHandler<ToggleScaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleScaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task Handle(ToggleScaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Scales.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Scale), request.id);

            entity.active = !entity.active;


            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
