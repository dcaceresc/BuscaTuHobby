using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Gunplas.Commands.DeleteGunpla;

public class DeleteGunplaCommand : IRequest
{
    public int id { get; set; }

    public class DeleteGunplaCommandHandler : IRequestHandler<DeleteGunplaCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteGunplaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteGunplaCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Gunplas.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Gunpla), request.id);
            }

            _context.Gunplas.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
