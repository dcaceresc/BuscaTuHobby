namespace Domain.Entities
{
    public class Serie : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int franchiseId { get; set; }
        public bool active { get; set; }
        
        public virtual Franchise Franchise { get; set; } = default!;
    }
}
