using Domain.Entities;

namespace Application.Maintainer.Categories.Queries.GetCategories;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string GroupName { get; set; } = default!;
    public bool ISActive { get; set; }



    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>().
                ForMember(d => d.GroupName, opt => opt.MapFrom(x => x.Group.GroupName));
        }
    }
}