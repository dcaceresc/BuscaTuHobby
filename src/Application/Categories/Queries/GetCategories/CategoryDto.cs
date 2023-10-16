using Domain.Entities;

namespace Application.Categories.Queries.GetCategories;

public class CategoryDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryDto>();
        }
    }
}

