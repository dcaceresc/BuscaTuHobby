namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Commune> Communes { get; }
        DbSet<Configuration> Configurations { get; }
        DbSet<Franchise> Franchises { get; }
        DbSet<Product> Products { get; }
        DbSet<ProductCategory> ProductCategories { get; }
        DbSet<ProductImage> ProductImages { get; }
        DbSet<Inventory> Inventories { get; }
        DbSet<Manufacturer> Manufacturers { get; }
        DbSet<Category> Categories { get; }
        DbSet<Serie> Series { get; }
        DbSet<Store> Stores { get; }
        DbSet<StoreAddress> StoresAddresses { get; }

        DbSet<User> Users { get; }
        DbSet<Region> Regions { get; }
        DbSet<Role> Roles { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<UserRole> UserRoles { get; }




        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
