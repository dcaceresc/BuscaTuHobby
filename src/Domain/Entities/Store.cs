namespace Domain.Entities
{
    public class Store : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string address { get; set; } = default!;
        public string webSite { get; set; } = default!;
        public int ranking { get; set; }
        public bool active { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; } = default!;

    }
}
