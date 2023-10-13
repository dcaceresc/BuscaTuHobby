using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Photos.Queries.GetPhotos;

public class PhotoVm : IMapFrom<Photo>
{
    public int id { get; set; }
    public int order { get; set; }
    public byte[] imageData { get; set; } = default!;
    public int gunplaId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Photo, PhotoVm>();
    }
}