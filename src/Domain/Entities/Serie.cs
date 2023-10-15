namespace Domain.Entities
{
    public class Serie : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int universeId { get; set; }
        public bool active { get; set; }
        public Universe Universe { get; set; } = default!;
        public ICollection<Product> Products { get; set; } = default!;
    }
}
