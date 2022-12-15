using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Grade> Grades { get; }
        DbSet<Gunpla> Gunplas { get; }
        DbSet<GunplaPrice> GunplaPrices { get; }
        DbSet<Manufacturer> Manufacturers { get; }
        DbSet<Photo> Photos { get; }
        DbSet<Sale> Sales { get; }
        DbSet<Scale> Scales { get; }
        DbSet<Serie> Series { get; }
        DbSet<Store> Stores { get; }
        DbSet<Universe> Universes { get; }



        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
