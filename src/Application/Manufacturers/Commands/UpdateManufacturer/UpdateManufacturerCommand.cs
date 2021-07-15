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

namespace Application.Manufacturers.Commands.UpdateManufacturer
{
    public class UpdateManufacturerCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public class UpdateManufacturerCommandHandler : IRequestHandler<UpdateManufacturerCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateManufacturerCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Manufacturers.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Manufacturer), request.Id);
                }

                entity.Name = request.Name;

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
