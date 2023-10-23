using Domain.Entities;

namespace Application.Maintainer.Franchises.Queries.GetFranchises;
public class FranchiseDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Franchise, FranchiseDto>();
        }
    }
}
