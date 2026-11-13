using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseForCreateDTO
{
    public PurchaseForCreateDTO()
    {
    }

    protected PurchaseForCreateDTO(
        string? purchaseDescription,
        bool wrapped,
        List<PurchaseItemDTO> purchaseItems)
    {
        PurchaseDescription = purchaseDescription;
        Wrapped = wrapped;
        PurchaseItems = purchaseItems ?? throw new ArgumentNullException(nameof(purchaseItems));
    }

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
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Province = province ?? throw new ArgumentNullException(nameof(province));
        CreditCardNumber = creditCardNumber ?? throw new ArgumentNullException(nameof(creditCardNumber));
        CCV = ccv ?? throw new ArgumentNullException(nameof(ccv));
        ExpirationDate = expirationDate ?? throw new ArgumentNullException(nameof(expirationDate));
        PurchaseItems = purchaseItems ?? throw new ArgumentNullException(nameof(purchaseItems));
        PurchaseDescription = purchaseDescription;
        Wrapped = wrapped;
    }

    [Required, StringLength(255, MinimumLength = 3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Street { get; set; } = null!;

    [Required, StringLength(255, MinimumLength = 3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string City { get; set; } = null!;

    [Required, StringLength(255, MinimumLength = 3)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Province { get; set; } = null!;

    [Required, CreditCard]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string CreditCardNumber { get; set; } = null!;

    [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "CCV must be 3 or 4 digits")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string CCV { get; set; } = null!;

    [Required, RegularExpression( @"^(0[1-9]|1[0-2])/([0-9]{2}|[0-9]{4})$", ErrorMessage = "Expiration date must be in MM/YY or MM/YYYY format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ExpirationDate { get; set; } = null!;

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<PurchaseItemDTO> PurchaseItems { get; set; } = null!;

    [StringLength(255)]
    public string? PurchaseDescription { get; set; }

    public bool Wrapped { get; set; }

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
               (PurchaseItems is null
                   ? other.PurchaseItems is null
                   : other.PurchaseItems is not null && PurchaseItems.SequenceEqual(other.PurchaseItems));
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
        if (PurchaseItems is not null)
        {
            foreach (var item in PurchaseItems)
            {
                hash.Add(item);
            }
        }

        return hash.ToHashCode();
    }
}
