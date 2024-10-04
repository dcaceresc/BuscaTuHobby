using Domain.Entities;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
    {

        public DbSet<SubMenu> SubMenus => Set<SubMenu>();
        public DbSet<Commune> Communes => Set<Commune>();
        public DbSet<Domain.Entities.Configuration> Configurations => Set<Domain.Entities.Configuration>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();
        public DbSet<Franchise> Franchises => Set<Franchise>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Region> Regions => Set<Region>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Serie> Series => Set<Serie>();
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<StoreAddress> StoresAddresses => Set<StoreAddress>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();


        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<SubMenu>(entity =>
            {
                entity.HasKey(e => e.SubMenuId);

                entity.Property(e => e.SubMenuId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.SubMenuName)
                .IsUnique();

                entity.Property(e => e.SubMenuName)
                .HasMaxLength(50);
            });

            builder.Entity<Commune>(entity =>
            {
                entity.HasKey(e => e.CommuneId);

                entity.Property(e => e.CommuneId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.CommuneName)
                .IsUnique();

                entity.Property(e => e.CommuneName)
                .HasMaxLength(100);
            });

            builder.Entity<Domain.Entities.Configuration>(entity =>
            {
                entity.HasKey(e => e.ConfigurationId);

                entity.Property(e => e.ConfigurationId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.ConfigurationName)
                .IsUnique();

                entity.Property(e => e.ConfigurationName)
                .HasMaxLength(100);

                entity.Property(e => e.ConfigurationValue)
                .HasMaxLength(4000);
            });

            builder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.FavoriteId);

                entity.Property(e => e.FavoriteId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.UserId)
                .IsUnique();

                entity.Property(e => e.UserId)
                .HasMaxLength(36);
            });

            builder.Entity<FavoriteProduct>(entity =>
            {
                entity.HasKey(e => new { e.FavoriteId, e.ProductId });

                entity.HasOne(d => d.Favorite)
                .WithMany(p => p.FavoriteProducts)
                .HasForeignKey(d => d.FavoriteId);

                entity.HasOne(d => d.Product)
                .WithMany(p => p.FavoriteProducts)
                .HasForeignKey(d => d.ProductId);
            });

            builder.Entity<Franchise>(entity =>
            {
                entity.HasKey(e => e.FranchiseId);

                entity.Property(e => e.FranchiseId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.FranchiseName)
                .IsUnique();

                entity.Property(e => e.FranchiseName)
                .HasMaxLength(50);
            });

            builder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.MenuId);

                entity.Property(e => e.MenuId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.MenuName)
                .IsUnique();

                entity.Property(e => e.MenuName)
                .HasMaxLength(50);
            });

            builder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.InventoryId);

                entity.Property(e => e.InventoryId)
                .HasDefaultValueSql("(newid())");

            });

            builder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(e => e.ManufacturerId);

                entity.Property(e => e.ManufacturerId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.ManufacturerName)
                .IsUnique();

                entity.Property(e => e.ManufacturerName)
                .HasMaxLength(50);
            });

            builder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.ProductId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.ProductName)
                .IsUnique();

                entity.Property(e => e.ProductName)
                .HasMaxLength(200);

                entity.Property(e => e.ProductTargetAge)
                .HasMaxLength(100);

                entity.Property(e => e.ProductSize)
                .HasMaxLength(100);

                entity.Property(e => e.ProductDescription);

            });

            builder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(e => new { e.CategoryId, e.ProductId });

                entity.HasOne(d => d.Category)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.CategoryId);

                entity.HasOne(d => d.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ProductId);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.ProductImageId);

                entity.Property(e => e.ProductImageId)
                .HasDefaultValueSql("(newid())");


            });

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.RefreshTokenId);

                entity.Property(e => e.RefreshTokenId)
                .HasDefaultValueSql("(newid())");

                entity.Property(e => e.RefreshTokenValue)
                .HasColumnType("varchar(32)");
            });

            builder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.RegionId);

                entity.Property(e => e.RegionId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.RegionName)
                .IsUnique();

                entity.Property(e => e.RegionName)
                .HasMaxLength(100);
            });


            builder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);

                entity.Property(e => e.ReviewId)
                .HasDefaultValueSql("(newid())");

                entity.Property(e => e.ReviewMessage)
                .HasMaxLength(1000);

            });

            builder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.RoleName)
                .IsUnique();

                entity.Property(e => e.RoleName)
                .HasMaxLength(50);
            });

            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.CategoryName)
                .IsUnique();

                entity.Property(e => e.CategoryName)
                .HasMaxLength(50);
            });

            builder.Entity<Serie>(entity =>
            {
                entity.HasKey(e => e.SerieId);

                entity.Property(e => e.SerieId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.SerieName)
                .IsUnique();

                entity.Property(e => e.SerieName)
                .HasMaxLength(50);
            });

            builder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.StoreId);

                entity.Property(e => e.StoreId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.StoreName)
                .IsUnique();

                entity.Property(e => e.StoreName)
                .HasMaxLength(50);

                entity.Property(e => e.StoreWebSite)
                .HasMaxLength(100);
            });

            builder.Entity<StoreAddress>(entity =>
            {
                entity.HasKey(e => e.StoreAddressId);

                entity.Property(e => e.StoreAddressId)
                .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Street)
                .HasMaxLength(100);

                entity.Property(e => e.ZipCode)
                .HasMaxLength(10);
            });

            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.Email)
                .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.PasswordHash);
                entity.Property(e => e.SecurityStamp);
                entity.Property(e => e.EmailConfirmed);
                entity.Property(e => e.LockoutEnabled);
                entity.Property(e => e.AccessFailedCount);
                entity.Property(e => e.LastLoginDate);
            });

            builder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(d => d.Role)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId);
            });

            ConfigureAuditableEntity<SubMenu>(builder);
            ConfigureAuditableEntity<Domain.Entities.Configuration>(builder);
            ConfigureAuditableEntity<Favorite>(builder);
            ConfigureAuditableEntity<FavoriteProduct>(builder);
            ConfigureAuditableEntity<Franchise>(builder);
            ConfigureAuditableEntity<Menu>(builder);
            ConfigureAuditableEntity<Inventory>(builder);
            ConfigureAuditableEntity<Manufacturer>(builder);
            ConfigureAuditableEntity<Product>(builder);
            ConfigureAuditableEntity<ProductCategory>(builder);
            ConfigureAuditableEntity<ProductImage>(builder);
            ConfigureAuditableEntity<RefreshToken>(builder);
            ConfigureAuditableEntity<Review>(builder);
            ConfigureAuditableEntity<Role>(builder);
            ConfigureAuditableEntity<Category>(builder);
            ConfigureAuditableEntity<Serie>(builder);
            ConfigureAuditableEntity<Store>(builder);
            ConfigureAuditableEntity<User>(builder);
            ConfigureAuditableEntity<UserRole>(builder);


            base.OnModelCreating(builder);
        }

        private static void ConfigureAuditableEntity<T>(ModelBuilder builder) where T : AuditableEntity
        {
            builder.Entity<T>()
                .Property(e => e.CreatedBy)
                .HasColumnType("varchar(30)");

            builder.Entity<T>()
                .Property(e => e.LastModifiedBy)
                .HasColumnType("varchar(30)");
        }


    }
}