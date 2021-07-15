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

namespace Application.Gunplas.Commands.UpdateGunpla
{
    public class UpdateGunplaCommand : IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GradeId { get; set; }
        public int ScaleId { get; set; }
        public int ManufacturerId { get; set; }
        public int SerieId { get; set; }
        public bool Base { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }

        public class UpdateGunplaCommandHandler : IRequestHandler<UpdateGunplaCommand>
        {
            private readonly IApplicationDbContext _context;

            public UpdateGunplaCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(UpdateGunplaCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Gunplas.FindAsync(request.Id);

                if (entity == null)
                {
                    throw new NotFoundException(nameof(Gunpla), request.Id);
                }

                entity.Name = request.Name;
                entity.GradeId = request.GradeId;
                entity.ScaleId = request.ScaleId;
                entity.ManufacturerId = request.ManufacturerId;
                entity.SerieId = request.SerieId;
                entity.Base = request.Base;
                entity.Description = request.Description;
                entity.ReleaseDate = request.ReleaseDate;


                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}
