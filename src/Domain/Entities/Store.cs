using Domain.Common;

namespace Domain.Entities
{
    public class Store : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string address { get; set; } = default!;
        public string webSite { get; set; } = default!;
        public int ranking { get; set; }

        public virtual ICollection<Sale> sale { get; set; } = default!;
        public virtual ICollection<GunplaPrice> gunplaPrice { get; set; } = default!;

    }
}
