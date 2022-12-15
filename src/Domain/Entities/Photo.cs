
using Domain.Common;

namespace Domain.Entities
{
    public class Photo : AuditableEntity
    {
        public int id { get; set; }
        public int order { get; set; }
        public byte[] imageData { get; set; } = default!;
        public int gunplaId { get; set; }
        public Gunpla gunpla { get; set; } = default!;


    }
}
