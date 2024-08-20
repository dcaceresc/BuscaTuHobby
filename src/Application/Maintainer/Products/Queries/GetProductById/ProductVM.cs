namespace Application.Maintainer.Products.Queries.GetProductById;

public class ProductVM
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public Guid ScaleId { get; set; }
    public Guid ManufacturerId { get; set; }
    public Guid FranchiseId { get; set; }
    public Guid SerieId { get; set; }
    public bool ProductHasBase { get; set; }
    public string ProductTargetAge { get; set; } = default!;
    public string ProductSize { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public DateTime ProductReleaseDate { get; set; }
    public IList<Guid> CategoryIds { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductVM>().
                ForMember(d => d.CategoryIds, opt => opt.MapFrom(s => s.ProductCategories.Select(cp => cp.Category.CategoryId).ToList()));
        }
    }
}