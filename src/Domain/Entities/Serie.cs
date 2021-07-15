using Domain.Common;
using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Entities
{
    public partial class Serie : AuditableEntity
    {
        public Serie()
        {
            Gunplas = new HashSet<Gunpla>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int UniverseId { get; set; }

        public virtual Universe Universe { get; set; }
        public virtual ICollection<Gunpla> Gunplas { get; set; }
    }
}
