using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Category> Categories { get; }
        DbSet<Product> Products { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Manufacturer> Manufacturers { get; }
        DbSet<Photo> Photos { get; }
        DbSet<Scale> Scales { get; }
        DbSet<Serie> Series { get; }
        DbSet<Store> Stores { get; }
        DbSet<SubCategory> SubCategories { get; }



        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
