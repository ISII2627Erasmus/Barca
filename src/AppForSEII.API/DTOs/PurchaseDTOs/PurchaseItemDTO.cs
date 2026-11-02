using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class PurchaseItemDTO
{
    public PurchaseItemDTO(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    [Range(1, int.MaxValue)]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is PurchaseItemDTO other &&
               ItemId == other.ItemId &&
               Quantity == other.Quantity;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ItemId, Quantity);
    }
}
