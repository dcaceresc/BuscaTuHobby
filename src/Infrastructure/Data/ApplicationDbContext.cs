using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Infrastructure.Identity;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions,
            ICurrentUserService currentUserService) : base(options, operationalStoreOptions)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
        public DbSet<Inventory> Inventories { get; set; } = default!;
        public DbSet<Manufacturer> Manufacturers { get; set; } = default!;
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Photo> Photos { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<Scale> Scales { get; set; } = default!;
        public DbSet<Serie> Series { get; set; } = default!;
        public DbSet<Store> Stores { get; set; } = default!;
        public DbSet<SubCategory> SubCategories { get; set; } = default!;
        public DbSet<SubCategoryProduct> SubCategoryProducts { get; set; } = default!;



        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId;
                        entry.Entity.Created = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId;
                        entry.Entity.LastModified = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<FavoriteProduct>()
            .HasKey(fp => new { fp.favoriteId, fp.productId });

            builder.Entity<FavoriteProduct>()
                .HasOne(fp => fp.Favorite)
                .WithMany(f => f.FavoriteProducts)
                .HasForeignKey(fp => fp.favoriteId);

            builder.Entity<FavoriteProduct>()
                .HasOne(fp => fp.Product)
                .WithMany(p => p.FavoriteProducts)
                .HasForeignKey(fp => fp.productId);


            builder.Entity<SubCategoryProduct>()
                .HasKey(scp => new { scp.subCategoryId, scp.productId });

            builder.Entity<SubCategoryProduct>()
                .HasOne(cp => cp.SubCategory)
                .WithMany(c => c.SubCategoryProducts)
                .HasForeignKey(cp => cp.subCategoryId);

            builder.Entity<SubCategoryProduct>()
                .HasOne(cp => cp.Product)
                .WithMany(p => p.SubCategoryProducts)
                .HasForeignKey(cp => cp.productId);

            base.OnModelCreating(builder);
        }


    }
}