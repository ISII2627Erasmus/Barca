using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseDetailDTO : PurchaseForCreateDTO
{
    public PurchaseDetailDTO(
        int id,
        DateTime purchaseDate,
        string? purchaseDescription,
        decimal totalPrice,
        bool wrapped,
        PurchaseUserDTO user,
        List<PurchaseItemDTO> purchaseItems) : base(purchaseDescription, wrapped, purchaseItems)
    {
        ArgumentNullException.ThrowIfNull(user);

        Id = id;
        PurchaseDate = purchaseDate;
        TotalPrice = totalPrice;
        User = user;
    }

    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalPrice { get; set; }
    public PurchaseUserDTO User { get; set; } = new();

    public override bool Equals(object? obj)
    {
        return obj is PurchaseDetailDTO other &&
               base.Equals(other) &&
               Id == other.Id &&
               PurchaseDate == other.PurchaseDate &&
               TotalPrice == other.TotalPrice &&
               User.Equals(other.User);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id, PurchaseDate, TotalPrice, User);
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
