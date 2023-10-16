namespace Domain.Entities
{
    public class Category : AuditableEntity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public bool active { get; set; }

        public virtual ICollection<SubCategory> SubCategories { get; set; } = default!;
    }
}
