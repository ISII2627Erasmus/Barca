using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

public class Purchase
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [ForeignKey(nameof(CustomerId))]
    public ApplicationUser Customer { get; set; } = null!;

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

    public string? Description { get; set; }

    public bool IsWrapped { get; set; } = false;

    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    [Required]
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}

public class PurchaseItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PurchaseId { get; set; }

    [ForeignKey(nameof(PurchaseId))]
    public Purchase Purchase { get; set; } = null!;

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item Item { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}