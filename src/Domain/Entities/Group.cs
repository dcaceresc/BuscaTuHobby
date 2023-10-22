namespace Domain.Entities
{
    public class Group : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public bool active { get; set; }

        public virtual ICollection<Category> Categories { get; set; } = default!;
    }
}
