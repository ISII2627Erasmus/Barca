using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

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
        SportForPurchaseDTO sport)
    {
        Id = id;
        Name = name;
        Brand = brand;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        Sport = sport;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }
    public SportForPurchaseDTO Sport { get; set; } = new();

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
               Sport.Equals(other.Sport);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Id,
            Name,
            Brand,
            Price,
            RecommendedAge,
            Gender,
            Size,
            Sport);
    }
}

public class SportForPurchaseDTO
{
    public SportForPurchaseDTO()
    {
    }

    public SportForPurchaseDTO(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        return obj is SportForPurchaseDTO other &&
               Id == other.Id &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }
}
