using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gunplas.Queries.GetGunplas
{
    public class GunplaVm : IMapFrom<Gunpla>
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

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Gunpla, GunplaVm>();
        }
    }
}
