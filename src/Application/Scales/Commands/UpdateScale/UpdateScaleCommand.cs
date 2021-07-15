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

namespace Application.Scales.Commands.UpdateScale
{
    public class UpdateScaleCommand : IRequest
    {
        public int Id { get; set; }
        public string  Name { get; set; }

        public class UpdateScaleCommandHandler : IRequestHandler<UpdateScaleCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateScaleCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateScaleCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Scales.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Scale), request.Id);
                }

                entity.Name = request.Name;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }

    }
}
