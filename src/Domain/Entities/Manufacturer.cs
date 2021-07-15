using Domain.Common;
using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Entities
{
    public partial class Manufacturer : AuditableEntity
    {
        public Manufacturer()
        {
            Gunplas = new HashSet<Gunpla>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Gunpla> Gunplas { get; set; }
    }
}
