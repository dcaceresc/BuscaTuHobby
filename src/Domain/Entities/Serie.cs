namespace Domain.Entities
{
    public class Serie : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public bool active { get; set; }
        public ICollection<Product> Products { get; set; } = default!;
    }
}
