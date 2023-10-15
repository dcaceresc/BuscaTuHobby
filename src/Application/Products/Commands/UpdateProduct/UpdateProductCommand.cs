
using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int gradeId { get; set; }
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int serieId { get; set; }
    public bool hasBase { get; set; }
    public string description { get; set; } = default!;
    public DateTime releaseDate { get; set; }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Products.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Product), request.id);
            }

            entity.name = request.name;
            entity.gradeId = request.gradeId;
            entity.scaleId = request.scaleId;
            entity.manufacturerId = request.manufacturerId;
            entity.serieId = request.serieId;
            entity.hasBase = request.hasBase;
            entity.description = request.description;
            entity.releaseDate = request.releaseDate;


            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
