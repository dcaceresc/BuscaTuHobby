using Domain.Common;

namespace Domain.Entities
{
    public class Serie : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int universeId { get; set; }
        public Universe universe { get; set; } = default!;
        public ICollection<Gunpla> gunplas { get; set; } = default!;
    }
}
