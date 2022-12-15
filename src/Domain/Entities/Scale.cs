
using Domain.Common;

namespace Domain.Entities
{
    public class Scale : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;

        public virtual ICollection<Gunpla> gunplas { get; set; } = default!;
    }
}
