using Domain.Common;

namespace Domain.Entities
{
    public class Gunpla : AuditableEntity
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
        public bool actve {  get; set; }

        public virtual Grade grade { get; set; } = default!;
        public virtual Manufacturer manufacturer { get; set; } = default!;
        public virtual Scale scale { get; set; } = default!;
        public virtual Serie serie { get; set; } = default!;

        public virtual ICollection<Photo> photos { get; set; } = default!;
        public virtual ICollection<GunplaPrice> gunplaPrice { get; set; } = default!;




    }
}
