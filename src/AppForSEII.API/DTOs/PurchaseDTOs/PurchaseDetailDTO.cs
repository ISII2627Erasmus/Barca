using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class GetPurchaseDTO
{
    public GetPurchaseDTO(
        int id,
        DateTime purchaseDate,
        string? purchaseDescription,
        decimal totalPrice,
        bool wrapped,
        PurchaseUserDTO user,
        List<PurchaseItemDetailDTO> purchaseItems)
    {
        Id = id;
        PurchaseDate = purchaseDate;
        PurchaseDescription = purchaseDescription;
        TotalPrice = totalPrice;
        Wrapped = wrapped;
        User = user;
        PurchaseItems = purchaseItems;
    }

    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PurchaseDescription { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Wrapped { get; set; }
    public PurchaseUserDTO User { get; set; } = new();
    public List<PurchaseItemDetailDTO> PurchaseItems { get; set; } = new();

    public override bool Equals(object? obj)
    {
        return obj is GetPurchaseDTO other &&
               Id == other.Id &&
               PurchaseDate == other.PurchaseDate &&
               PurchaseDescription == other.PurchaseDescription &&
               TotalPrice == other.TotalPrice &&
               Wrapped == other.Wrapped &&
               User.Equals(other.User) &&
               PurchaseItems.SequenceEqual(other.PurchaseItems);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(PurchaseDate);
        hash.Add(PurchaseDescription);
        hash.Add(TotalPrice);
        hash.Add(Wrapped);
        hash.Add(User);
        foreach (var item in PurchaseItems)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}

public class PurchaseUserDTO
{
    public PurchaseUserDTO()
    {
    }

    public PurchaseUserDTO(string? name, string? surname)
    {
        Name = name;
        Surname = surname;
    }

    public string? Name { get; set; }
    public string? Surname { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PurchaseUserDTO other &&
               Name == other.Name &&
               Surname == other.Surname;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Surname);
    }
}

public class PurchaseItemDetailDTO
{
    public PurchaseItemDetailDTO()
    {
    }

    public PurchaseItemDetailDTO(
        int itemId,
        string name,
        string brand,
        decimal price,
        int? recommendedAge,
        Gender? gender,
        string? size,
        int quantity)
    {
        ItemId = itemId;
        Name = name;
        Brand = brand;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        Quantity = quantity;
    }

    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }
    public int Quantity { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PurchaseItemDetailDTO other &&
               ItemId == other.ItemId &&
               Name == other.Name &&
               Brand == other.Brand &&
               Price == other.Price &&
               RecommendedAge == other.RecommendedAge &&
               Gender == other.Gender &&
               Size == other.Size &&
               Quantity == other.Quantity;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            ItemId,
            Name,
            Brand,
            Price,
            RecommendedAge,
            Gender,
            Size,
            Quantity);
    }
}
