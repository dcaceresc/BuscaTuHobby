namespace Application.Maintainer.Categories.Queries.GetCategoryById;

public class CategoryVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int groupId { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryVM>();
        }
    }
}
