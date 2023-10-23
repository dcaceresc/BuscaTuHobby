using Domain.Entities;

namespace Application.Maintainer.Products.Queries.GetProducts;

public class ProductDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string scaleName { get; set; } = default!;
    public string manufacturerName { get; set; } = default!;
    public string franchiseName { get; set; } = default!;
    public string? serieName { get; set; }
    public IList<string> categories { get; set; } = default!;
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>().
                ForMember(d => d.scaleName, opt => opt.MapFrom(s => s.Scale.name)).
                ForMember(d => d.manufacturerName, opt => opt.MapFrom(s => s.Manufacturer.name)).
                ForMember(d => d.franchiseName, opt => opt.MapFrom(s => s.Franchise.name)).
                ForMember(d => d.serieName, opt => opt.MapFrom(s => s.Serie!.name)).
                ForMember(d => d.categories, opt => opt.MapFrom(s => s.CategoryProducts.Select(cp => cp.Category.name).ToList()));
        }
    }
}