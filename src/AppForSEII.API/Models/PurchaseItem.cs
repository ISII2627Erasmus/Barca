using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AppForSEII.API.Models;

[PrimaryKey(nameof(ItemId), nameof(PurchaseId))]
public class PurchaseItem
{
    public PurchaseItem() { }

    public PurchaseItem(Purchase purchase, Item item, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(purchase);
        ArgumentNullException.ThrowIfNull(item);
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Purchase = purchase;
        PurchaseId = purchase.Id;
        Item = item;
        ItemId = item.Id;
        Price = item.Price;
        Quantity = quantity;
    }

    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    [Precision(18, 2), Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
