
using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Gunplas.Queries.GetGunplas;

public class GunplaVm : IMapFrom<Gunpla>
{
    public int id { get; set; }
    public string name { get; set; }
    public int gradeId { get; set; }
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int serieId { get; set; }
    public bool hasBase { get; set; }
    public string description { get; set; }
    public DateTime releaseDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Gunpla, GunplaVm>();
    }
}