using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseForCreateDTO
{

    public PurchaseForCreateDTO(
        string street,
        string city,
        string province,
        string creditCardNumber,
        string ccv,
        string expirationDate,
        string? purchaseDescription,
        bool wrapped,
        List<PurchaseItemDTO> purchaseItems)
    {
        Street = street;
        City = city;
        Province = province;
        CreditCardNumber = creditCardNumber;
        CCV = ccv;
        ExpirationDate = expirationDate;
        PurchaseDescription = purchaseDescription;
        Wrapped = wrapped;
        PurchaseItems = purchaseItems;
    }

    [Required, StringLength(255, MinimumLength = 3)]
    public string Street { get; set; } = string.Empty;

    [Required, StringLength(255, MinimumLength = 3)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(255, MinimumLength = 3)]
    public string Province { get; set; } = string.Empty;

    [Required, CreditCard]
    public string CreditCardNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "CCV must be 3 or 4 digits")]
    public string CCV { get; set; } = string.Empty;

    [Required, RegularExpression(
        @"^(0[1-9]|1[0-2])\/?([0-9]{2}|[0-9]{4})$",
        ErrorMessage = "Expiration date must be in MM/YY or MM/YYYY format")]
    public string ExpirationDate { get; set; } = string.Empty;

    [StringLength(255)]
    public string? PurchaseDescription { get; set; }

    public bool Wrapped { get; set; }

    [Required]
    public List<PurchaseItemDTO> PurchaseItems { get; set; } = new();

    public override bool Equals(object? obj)
    {
        return obj is PurchaseForCreateDTO other &&
               Street == other.Street &&
               City == other.City &&
               Province == other.Province &&
               CreditCardNumber == other.CreditCardNumber &&
               CCV == other.CCV &&
               ExpirationDate == other.ExpirationDate &&
               PurchaseDescription == other.PurchaseDescription &&
               Wrapped == other.Wrapped &&
               PurchaseItems.SequenceEqual(other.PurchaseItems);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Street);
        hash.Add(City);
        hash.Add(Province);
        hash.Add(CreditCardNumber);
        hash.Add(CCV);
        hash.Add(ExpirationDate);
        hash.Add(PurchaseDescription);
        hash.Add(Wrapped);
        foreach (var item in PurchaseItems)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}
