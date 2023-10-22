using Domain.Entities;

namespace Application.Maintainer.Groups.Commands.CreateGroup;
public class CreateGroupCommand : IRequest<int>
{
    public string name { get; set; } = default!;


    public class CreateCategoryCommandHandler : IRequestHandler<CreateGroupCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateCategoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = new Group
            {
                name = request.name,
                active = true
            };

            _context.Groups.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}