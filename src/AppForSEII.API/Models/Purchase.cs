using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppForSEII.API.Models;

public class Purchase
{
	[Key]
	public int Id { get; set; }
	
	[Required]
	public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
	
	public string? PurchaseDescription { get; set; }
	
	[Required]
	[Range(0, double.MaxValue)]
	public double TotalPrice { get; set; }
	
	[Required]
	[StringLength(255, MinimumLength = 3)]
	public string Street { get; set; } = string.Empty;
	
	[Required]
	[StringLength(255, MinimumLength = 3)]
	public string City { get; set; } = string.Empty;
	
	[Required]
	[StringLength(255, MinimumLength = 3)]
	public string Province { get; set; } = string.Empty;
	
	[Required]
	[CreditCard]
	public string CreditCardNumber { get; set; } = string.Empty;
	
	[Required]
	[RegularExpression(@"^\d{3,4}$", ErrorMessage = "CCV must be 3 or 4 digits.")]
	public string CCV { get; set; } = string.Empty;
	
	[Required]
	[RegularExpression(
		@"^(0[1-9]|1[0-2])\/?([0-9]{2}|[0-9]{4})$",
					   ErrorMessage = "Expiration date must use MM/YY or MM/YYYY format.")]
					   public string ExpirationDate { get; set; } = string.Empty;
					   
					   public bool Wrapped { get; set; } = false;
					   
					   [Required]
					   public PurchaseStatus PurchaseStatus { get; set; } = PurchaseStatus.Created;
					   
					   [Required]
					   public string UserId { get; set; } = string.Empty;
					   
					   [ForeignKey(nameof(UserId))]
					   public ApplicationUser User { get; set; } = null!;
					   
					   public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}