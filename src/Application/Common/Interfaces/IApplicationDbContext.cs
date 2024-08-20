namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Group> Groups { get; }
        DbSet<Franchise> Franchises { get; }
        DbSet<Product> Products { get; }
        DbSet<ProductCategory> ProductCategories { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Manufacturer> Manufacturers { get; }
        DbSet<Scale> Scales { get; }
        DbSet<Serie> Series { get; }
        DbSet<Store> Stores { get; }
        DbSet<Category> Categories { get; }




        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
