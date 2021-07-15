using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Grade> Grades { get; set; }
        DbSet<Gunpla> Gunplas { get; set; }
        DbSet<Manufacturer> Manufacturers { get; set; }
        DbSet<Scale> Scales { get; set; }
        DbSet<Serie> Series { get; set; }
        DbSet<Universe> Universes { get; set; }

        DbSet<Photo> Photos { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
