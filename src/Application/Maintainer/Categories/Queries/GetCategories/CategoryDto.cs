using Domain.Entities;

namespace Application.Maintainer.Categories.Queries.GetCategories;

public class CategoryDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string groupName { get; set; } = default!;
    public bool active { get; set; }



    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>().
                ForMember(d => d.groupName, opt => opt.MapFrom(x => x.Group.name));
        }
    }
}