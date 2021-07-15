using Domain.Common;
using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Entities
{
    public partial class Gunpla : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GradeId { get; set; }
        public int ScaleId { get; set; }
        public int ManufacturerId { get; set; }
        public int SerieId { get; set; }
        public bool Base { get; set; }
        public string Description { get; set; }
        public DateTime ReleaseDate { get; set; }

        public virtual Grade Grade { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Scale Scale { get; set; }
        public virtual Serie Serie { get; set; }

        public ICollection<Photo> Photos { get; set; }
    }
}
