namespace Domain.Entities
{
    public class Grade : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string acronym { get; set; } = default!;
        public bool active { get; set; }

        public virtual ICollection<Gunpla> Gunplas { get; set; } = default!;
    }
}
