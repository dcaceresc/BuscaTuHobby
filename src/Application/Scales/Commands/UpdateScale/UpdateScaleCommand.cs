using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Scales.Commands.UpdateScale;

public class UpdateScaleCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; }

    public class UpdateScaleCommandHandler : IRequestHandler<UpdateScaleCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateScaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UpdateScaleCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Scales.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Scale), request.id);
            }

            entity.name = request.name;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

}
