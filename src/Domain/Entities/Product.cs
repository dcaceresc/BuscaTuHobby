namespace Domain.Entities
{
    public class Product : AuditableEntity
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
        public bool actve { get; set; }

        public virtual Grade Grade { get; set; } = default!;
        public virtual Manufacturer Manufacturer { get; set; } = default!;
        public virtual Scale Scale { get; set; } = default!;
        public virtual Serie Serie { get; set; } = default!;

        public virtual ICollection<Photo> Photos { get; set; } = default!;
        public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = default!;

    }
}
