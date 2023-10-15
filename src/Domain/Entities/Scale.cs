namespace Domain.Entities
{
    public class Scale : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string acronym { get; set; } = default!;
        public bool active { get; set; }

        public virtual ICollection<Product> Products { get; set; } = default!;
    }
}
