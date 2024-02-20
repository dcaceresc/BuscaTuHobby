using Domain.Entities;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();
        public DbSet<Franchise> Franchises => Set<Franchise>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Scale> Scales => Set<Scale>();
        public DbSet<Serie> Series => Set<Serie>();
        public DbSet<Store> Stores => Set<Store>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();


        protected override void OnModelCreating(ModelBuilder builder)
        {

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

            builder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.GroupId);

                entity.Property(e => e.GroupId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.GroupName)
                .IsUnique();

                entity.Property(e => e.GroupName)
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
                .HasMaxLength(50);

                entity.Property(e => e.ProductTargetAge)
                .HasMaxLength(50);

                entity.Property(e => e.ProductSize)
                .HasMaxLength(50);

                entity.Property(e => e.ProductDescription)
                .HasMaxLength(1000);

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

                entity.Property(e => e.ProductImagePath)
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

            builder.Entity<Scale>(entity =>
            {
                entity.HasKey(e => e.ScaleId);

                entity.Property(e => e.ScaleId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.ScaleName)
                .IsUnique();

                entity.Property(e => e.ScaleName)
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

                entity.Property(e => e.StoreAddress)
                .HasMaxLength(100);

                entity.Property(e => e.StoreWebSite)
                .HasMaxLength(100);
            });

            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())");

                entity.HasIndex(e => e.Email)
                .IsUnique();

                entity.Property(e => e.Email)
                .HasMaxLength(50);

                entity.Property(e => e.FirstName)
                .HasMaxLength(50);

                entity.Property(e => e.LastName)
                .HasMaxLength(50);

                entity.Property(e => e.Password)
                .HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50);

                entity.Property(e => e.UserName)
                .HasMaxLength(50);
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

            ConfigureAuditableEntity<Category>(builder);
            ConfigureAuditableEntity<Favorite>(builder);
            ConfigureAuditableEntity<FavoriteProduct>(builder);
            ConfigureAuditableEntity<Franchise>(builder);
            ConfigureAuditableEntity<Group>(builder);
            ConfigureAuditableEntity<Inventory>(builder);
            ConfigureAuditableEntity<Manufacturer>(builder);
            ConfigureAuditableEntity<Product>(builder);
            ConfigureAuditableEntity<ProductCategory>(builder);
            ConfigureAuditableEntity<ProductImage>(builder);
            ConfigureAuditableEntity<Review>(builder);
            ConfigureAuditableEntity<Role>(builder);
            ConfigureAuditableEntity<Scale>(builder);
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