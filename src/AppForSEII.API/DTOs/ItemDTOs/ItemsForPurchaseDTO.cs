using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.ItemDTOs;

public class ItemsForPurchaseDTO
{
    public ItemsForPurchaseDTO(
        int id,
        string name,
        string brand,
        decimal price,
        int? recommendedAge,
        Gender? gender,
        string? size,
        string sportName,
        int quantityAvailableForPurchase)
    {
        ArgumentNullException.ThrowIfNull(sportName);

        Id = id;
        Name = name;
        Brand = brand;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        SportName = sportName;
        QuantityAvailableForPurchase = quantityAvailableForPurchase;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }
    public string SportName { get; set; } = string.Empty;
    public int QuantityAvailableForPurchase { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ItemsForPurchaseDTO other &&
               Id == other.Id &&
               Name == other.Name &&
               Brand == other.Brand &&
               Price == other.Price &&
               RecommendedAge == other.RecommendedAge &&
               Gender == other.Gender &&
               Size == other.Size &&
               SportName == other.SportName &&
               QuantityAvailableForPurchase == other.QuantityAvailableForPurchase;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(Name);
        hash.Add(Brand);
        hash.Add(Price);
        hash.Add(RecommendedAge);
        hash.Add(Gender);
        hash.Add(Size);
        hash.Add(SportName);
        hash.Add(QuantityAvailableForPurchase);
        return hash.ToHashCode();
    }
}
