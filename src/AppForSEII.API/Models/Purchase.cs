using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

public class Purchase
{
    public Purchase() { }

    public Purchase(
        ApplicationUser user,
        string street,
        string city,
        string province,
        string creditCardNumber,
        string ccv,
        string expirationDate,
        string? purchaseDescription = null,
        bool isWrapped = false)
    {
        ArgumentNullException.ThrowIfNull(user);

        User = user;
        UserId = user.Id;
        Street = street;
        City = city;
        Province = province;
        CreditCardNumber = creditCardNumber;
        CCV = ccv;
        ExpirationDate = expirationDate;
        PurchaseDescription = purchaseDescription;
        IsWrapped = isWrapped;
        PurchaseDate = DateTime.UtcNow;
        TotalPrice = 0m;
        PurchaseStatus = global::AppForSEII.API.Models.PurchaseStatus.Created;
    }
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [CreditCard]
    public string CreditCardNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CCV must be 3 or 4 digits")]
    public string CCV { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2}|[0-9]{4})$", ErrorMessage = "Invalid Expiration Date (MM/YY)")]
    public string ExpirationDate { get; set; } = string.Empty;
    public bool IsWrapped { get; set; } = false;
    
    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    public string? PurchaseDescription { get; set; }

    public PurchaseStatus PurchaseStatus { get; set; }= global::AppForSEII.API.Models.PurchaseStatus.Created;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}