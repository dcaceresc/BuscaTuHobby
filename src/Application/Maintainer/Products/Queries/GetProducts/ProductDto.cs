using Domain.Entities;

namespace Application.Maintainer.Products.Queries.GetProducts;

public class ProductDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int gradeId { get; set; }
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int serieId { get; set; }
    public bool hasBase { get; set; }
    public string description { get; set; } = default!;
    public DateTime releaseDate { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Product, ProductDto>();
        }
    }
}