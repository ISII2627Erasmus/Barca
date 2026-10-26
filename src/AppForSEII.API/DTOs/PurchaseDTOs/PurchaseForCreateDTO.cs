using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseForCreateDTO
{
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
    public List<PurchaseItemForCreateDTO> PurchaseItems { get; set; } = new();
}

public class PurchaseItemForCreateDTO
{
    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
