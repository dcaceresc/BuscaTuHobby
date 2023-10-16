using Domain.Entities;

namespace Application.Categories.Queries.GetSubCategoriesByCategory;

public class SubCategoryDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SubCategory, SubCategoryDto>();
        }
    }
}
