using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseForCreateDTO : IValidatableObject
{
    public PurchaseForCreateDTO()
    {
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
        PurchaseDescription = purchaseDescription;
        Wrapped = wrapped;
        PurchaseItems = purchaseItems ?? throw new ArgumentNullException(nameof(purchaseItems));
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
        @"^(0[1-9]|1[0-2])/([0-9]{2}|[0-9]{4})$",
        ErrorMessage = "Expiration date must be in MM/YY or MM/YYYY format")]
    public string ExpirationDate { get; set; } = string.Empty;

    [StringLength(255)]
    public string? PurchaseDescription { get; set; }

    public bool Wrapped { get; set; }

    [Required]
    public List<PurchaseItemDTO> PurchaseItems { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // The property-level RegularExpression checks the format. This object-level
        // rule adds the date constraint: a card stays valid through its expiry month.
        if (string.IsNullOrWhiteSpace(ExpirationDate) ||
            ExpirationDate.Length is not (5 or 7) ||
            ExpirationDate[2] != '/' ||
            !int.TryParse(ExpirationDate.AsSpan(0, 2), out var month) ||
            !int.TryParse(ExpirationDate.AsSpan(3), out var year) ||
            month is < 1 or > 12)
        {
            yield break;
        }

        if (ExpirationDate.Length == 5)
        {
            // Two-digit card expiry years use 20xx (for example, 30 means 2030).
            year += 2000;
        }

        if (year < 1)
        {
            yield return new ValidationResult(
                "The expiration date is invalid.",
                new[] { nameof(ExpirationDate) });
            yield break;
        }

        var expirationMonthEnd = new DateTime(
            year,
            month,
            DateTime.DaysInMonth(year, month));

        if (expirationMonthEnd < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult(
                "The credit card has expired.",
                new[] { nameof(ExpirationDate) });
        }
    }

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
