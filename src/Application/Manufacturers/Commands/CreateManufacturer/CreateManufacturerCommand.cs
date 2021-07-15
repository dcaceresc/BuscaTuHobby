using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Manufacturers.Commands.CreateManufacturer
{
    public class CreateManufacturerCommand : IRequest<int>
    {
        public string Name { get; set; }

        public class CreateManufacturerCommandHandler : IRequestHandler<CreateManufacturerCommand, int>
        {
            private readonly IApplicationDbContext _context;

            public CreateManufacturerCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<int> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
            {
                var entity = new Manufacturer
                {
                    Name = request.Name
                };

                _context.Manufacturers.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return entity.Id;
            }
        }
    }
}
