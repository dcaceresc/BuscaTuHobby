using Domain.Entities;

namespace Application.Maintainer.Products.Queries.GetProductById;

public class ProductVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int franchiseId { get; set; }
    public int? serieId { get; set; }
    public bool hasBase { get; set; }
    public string targetAge { get; set; } = default!;
    public string size { get; set; } = default!;
    public string description { get; set; } = default!;
    public DateTime releaseDate { get; set; }
    public IList<int> categories { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductVM>().
                ForMember(d => d.categories, opt => opt.MapFrom(s => s.CategoryProducts.Select(cp => cp.Category.id).ToList()));
        }
    }
}