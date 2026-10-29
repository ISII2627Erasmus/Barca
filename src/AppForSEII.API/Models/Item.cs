using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[Index(nameof(Name), IsUnique = true)]
public class Item
{
    public Item() { }

    public Item(
        string name,
        string brand,
        decimal price,
        int? recommendedAge,
        Gender? gender,
        string? size,
        int quantityAvailableForPurchase,
        Sport sport)
    {
        ArgumentNullException.ThrowIfNull(sport);

        Name = name;
        Brand = brand;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        QuantityAvailableForPurchase = quantityAvailableForPurchase;
        Sport = sport;
        SportId = sport.Id;
    }

    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Precision(18, 2), Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Price { get; set; }

    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityAvailableForPurchase { get; set; }

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}
