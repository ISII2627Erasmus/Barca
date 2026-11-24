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
        string? customerName,
        string? customerSurname,
        List<PurchaseItemDTO> purchaseItems,
        string customerUserName) : base(purchaseDescription, wrapped, purchaseItems, customerUserName)
    {
        Id = id;
        PurchaseDate = purchaseDate;
        TotalPrice = totalPrice;
        CustomerName = customerName;
        CustomerSurname = customerSurname;
    }

    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerSurname { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PurchaseDetailDTO other &&
               base.Equals(other) &&
               Id == other.Id &&
               PurchaseDate == other.PurchaseDate &&
               TotalPrice == other.TotalPrice &&
               CustomerName == other.CustomerName &&
               CustomerSurname == other.CustomerSurname;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id, PurchaseDate, TotalPrice, CustomerName, CustomerSurname);
    }
}
