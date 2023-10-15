namespace Domain.Entities
{
    public class Photo : AuditableEntity
    {
        public int id { get; set; }
        public int order { get; set; }
        public byte[] imageData { get; set; } = default!;
        public int productId { get; set; }
        public Product Product { get; set; } = default!;


    }
}
