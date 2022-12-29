
using Domain.Common;

namespace Domain.Entities
{
    public class Sale : AuditableEntity
    {
        public int id { get; set; }
        public int gunplaId { get; set; }
        public int storeId { get; set; }
        public int price { get; set; }

        public virtual Gunpla gunpla { get; set; } = default!;
        public virtual Store store { get; set; } = default!;

    }
}
