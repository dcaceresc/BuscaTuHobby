﻿using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Manufacturers.Queries.GetManufacturers
{
    public class ManufacturerVm : IMapFrom<Manufacturer>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Manufacturer, ManufacturerVm>();
        }
    }
}
