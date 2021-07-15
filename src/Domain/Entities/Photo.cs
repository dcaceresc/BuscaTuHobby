using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Photo : AuditableEntity
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public byte[] ImageData { get; set; }

        public int GunplaId { get; set; }
        public Gunpla Gunpla { get; set; }


    }
}
