using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Photos.Queries.GetPhotos
{
    public class PhotoVm : IMapFrom<Photo>
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public byte[] ImageData { get; set; }
        public int GunplaId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Photo, PhotoVm>();
        }
    }
}
