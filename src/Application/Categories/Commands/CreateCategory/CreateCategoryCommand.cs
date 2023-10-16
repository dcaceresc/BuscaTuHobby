using Domain.Entities;

namespace Application.Categories.Commands.CreateCategory;
public class CreateCategoryCommand : IRequest<int>
{
    public string name { get; set; } = default!;


    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateCategoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = new Category
            {
                name = request.name,
                active = true
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}