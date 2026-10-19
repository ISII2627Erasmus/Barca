using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public sealed class PurchaseForCreateDTO
{
    public PurchaseForCreateDTO(
        string street,
        string city,
        string province,
        string creditCardNumber,
        string ccv,
        string expirationDate,
        string? purchaseDescription = null,
        bool wrapped = false,
        List<PurchaseItemForCreateDTO>? purchaseItems = null)
    {
        Street = street;
        City = city;
        Province = province;
        CreditCardNumber = creditCardNumber;
        CCV = ccv;
        ExpirationDate = expirationDate;
        PurchaseDescription = purchaseDescription;
        Wrapped = wrapped;
        PurchaseItems = purchaseItems ?? new List<PurchaseItemForCreateDTO>();
    }

    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Street { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string City { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Province { get; set; }

    [Required]
    [CreditCard]
    public string CreditCardNumber { get; set; }

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CCV must be 3 or 4 digits")]
    public string CCV { get; set; }

    [Required]
    [RegularExpression(
        @"^(0[1-9]|1[0-2])\/?([0-9]{2}|[0-9]{4})$",
        ErrorMessage = "Expiration date must be in MM/YY or MM/YYYY format")]
    public string ExpirationDate { get; set; }

    [StringLength(255)]
    public string? PurchaseDescription { get; set; }

    public bool Wrapped { get; set; }

    [Required]
    public List<PurchaseItemForCreateDTO> PurchaseItems { get; set; }

}

public sealed class PurchaseItemForCreateDTO
{
    public PurchaseItemForCreateDTO(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
