using Domain.Common;

namespace Domain.Entities
{
    public class Manufacturer:AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public virtual ICollection<Gunpla> gunplas { get; set; } = default!;
    }
}
