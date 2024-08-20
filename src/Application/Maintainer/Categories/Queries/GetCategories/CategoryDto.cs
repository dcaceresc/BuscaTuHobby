namespace Application.Maintainer.Categories.Queries.GetCategories;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string GroupName { get; set; } = default!;
    public bool IsActive { get; set; }



    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>().
                ForMember(d => d.GroupName, opt => opt.MapFrom(x => x.Group.GroupName));
        }
    }
}