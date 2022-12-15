using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Scales.Commands.DeleteScale;

public class DeleteScaleCommand : IRequest
{
    public int id { get; set; }

    public class DeleteScaleCommandHandler : IRequestHandler<DeleteScaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteScaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Unit> Handle(DeleteScaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Scales.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Scale), request.id);
            }

            _context.Scales.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
