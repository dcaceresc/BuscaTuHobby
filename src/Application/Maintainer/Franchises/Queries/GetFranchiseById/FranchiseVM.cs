using Domain.Entities;

namespace Application.Maintainer.Franchises.Queries.GetFranchiseById;
public class FranchiseVM
{
    public Guid FranchiseId { get; set; }
    public string FranchiseName { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Franchise, FranchiseVM>();
        }
    }
}
