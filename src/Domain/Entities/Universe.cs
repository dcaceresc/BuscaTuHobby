using Domain.Common;

namespace Domain.Entities
{
    public class Universe : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string acronym { get; set; } = default!;
        public bool active { get; set; }
        public virtual ICollection<Serie> serie { get; set; } = default!;
    }
}
