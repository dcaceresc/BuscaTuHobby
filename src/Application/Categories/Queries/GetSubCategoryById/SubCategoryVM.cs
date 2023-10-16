using Domain.Entities;

namespace Application.Categories.Queries.GetSubCategoryById;

public class SubCategoryVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int categoryId { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<SubCategory, SubCategoryVM>();
        }
    }
}
