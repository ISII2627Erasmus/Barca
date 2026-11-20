using System.ComponentModel.DataAnnotations;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseItemDTO
{
    public PurchaseItemDTO()
    {}

    public PurchaseItemDTO(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    public PurchaseItemDTO(
        int itemId,
        string name,
        string brand,
        decimal price,
        int? recommendedAge,
        Gender? gender,
        string? size,
        string sportName,
        int quantity)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(brand);
        ArgumentNullException.ThrowIfNull(sportName);

        ItemId = itemId;
        Name = name;
        Brand = brand;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        SportName = sportName;
        Quantity = quantity;
    }

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string? Name { get; set; }
    public string? Brand { get; set; }
    public decimal Price { get; set; }
    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }
    public string? SportName { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PurchaseItemDTO other &&
               ItemId == other.ItemId &&
               Quantity == other.Quantity &&
               Name == other.Name &&
               Brand == other.Brand &&
               Price == other.Price &&
               RecommendedAge == other.RecommendedAge &&
               Gender == other.Gender &&
               Size == other.Size &&
               SportName == other.SportName;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ItemId);
        hash.Add(Quantity);
        hash.Add(Name);
        hash.Add(Brand);
        hash.Add(Price);
        hash.Add(RecommendedAge);
        hash.Add(Gender);
        hash.Add(Size);
        hash.Add(SportName);
        return hash.ToHashCode();
    }
}
