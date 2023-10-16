using Domain.Entities;

namespace Application.Categories.Queries.GetCategoryById
{
    public class CategoryVM
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
    }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Category, CategoryVM>();
        }
    }
}