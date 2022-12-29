using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class GunplaPrice
{

    public int gunplaId { get; set; }
    public Gunpla Gunpla { get; set; } = default!;

    public int storeId { get; set; }
    public Store StoreStore { get; set; } = default!;

    public int price { get; set; }


}
