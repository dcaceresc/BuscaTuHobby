using Application.Common.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Gunplas.Commands.CreateGunpla
{
    public class CreateGunplaCommand : IRequest<int>
    {
        public string Name { get; set; }
        public int GradeId { get; set; }
        public int ScaleId { get; set; }
        public int ManufacturerId { get; set; }
        public int SerieId { get; set; }
        public bool Base { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }

        public class CreateGunplaCommandHandler : IRequestHandler<CreateGunplaCommand, int>
        {
            private readonly IApplicationDbContext _context;

            public CreateGunplaCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<int> Handle(CreateGunplaCommand request, CancellationToken cancellationToken)
            {
                var entity = new Gunpla
                {
                    Name = request.Name,
                    GradeId = request.GradeId,
                    ScaleId = request.ScaleId,
                    ManufacturerId = request.ManufacturerId,
                    SerieId = request.SerieId,
                    Base = request.Base,
                    Description = request.Description,
                    ReleaseDate = request.ReleaseDate
                };

                _context.Gunplas.Add(entity);


                await _context.SaveChangesAsync(cancellationToken);

                return entity.Id;
            }
        }
    }
}
