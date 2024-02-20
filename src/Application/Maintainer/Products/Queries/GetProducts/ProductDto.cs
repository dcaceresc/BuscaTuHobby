using Domain.Entities;

namespace Application.Maintainer.Products.Queries.GetProducts;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ScaleName { get; set; } = default!;
    public string ManufacturerName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;
    public string SerieName { get; set; } = default!;
    public IList<string> Categories { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>().
                ForMember(d => d.ScaleName, opt => opt.MapFrom(s => s.Scale.ScaleName)).
                ForMember(d => d.ManufacturerName, opt => opt.MapFrom(s => s.Manufacturer.ManufacturerName)).
                ForMember(d => d.FranchiseName, opt => opt.MapFrom(s => s.Franchise.FranchiseName)).
                ForMember(d => d.SerieName, opt => opt.MapFrom(s => s.Serie!.SerieName)).
                ForMember(d => d.Categories, opt => opt.MapFrom(s => s.ProductCategories.Select(cp => cp.Category.CategoryName).ToList()));
        }
    }
}