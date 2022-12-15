using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Gunplas.Commands.CreateGunpla;

public class CreateGunplaCommand : IRequest<int>
{
    public string name { get; set; }
    public int gradeId { get; set; }
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int serieId { get; set; }
    public bool hasBase { get; set; }
    public string description { get; set; }
    public DateTime releaseDate { get; set; }

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
                name = request.name,
                gradeId = request.gradeId,
                scaleId = request.scaleId,
                manufacturerId = request.manufacturerId,
                serieId = request.serieId,
                hasBase = request.hasBase,
                description = request.description,
                releaseDate = request.releaseDate
            };

            _context.Gunplas.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }


}

