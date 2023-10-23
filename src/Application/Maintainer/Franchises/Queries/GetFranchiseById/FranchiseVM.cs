using Domain.Entities;

namespace Application.Maintainer.Franchises.Queries.GetFranchiseById;
public class FranchiseVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int franchiseId { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Franchise, FranchiseVM>();
        }
    }
}
