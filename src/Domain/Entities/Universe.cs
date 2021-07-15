using Domain.Common;
using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Entities
{
    public partial class Universe : AuditableEntity
    {
        public Universe()
        {
            Series = new HashSet<Serie>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Serie> Series { get; set; }
    }
}
