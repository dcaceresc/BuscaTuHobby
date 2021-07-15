using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Series.Queries.GetSeries
{
    public class SerieVm : IMapFrom<Serie>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UniverseId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Serie, SerieVm>();
        }
    }
}
