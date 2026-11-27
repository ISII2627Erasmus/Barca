using AppForSEII.Web.API;

namespace AppForSEII.Web;

/// <summary>
/// Holds a user's selected purchase items for the lifetime of their Blazor circuit.
/// </summary>
public class PurchaseStateContainer
{
    public List<ItemsForPurchaseDTO> SelectedItems { get; private set; } = new();

    public int SelectedCount => SelectedItems.Count;

    // At this stage, each selected item contributes one unit. The next purchase step
    // can collect the final quantity and recalculate the total.
    public decimal TotalPrice => SelectedItems.Sum(item => Convert.ToDecimal(item.Price));

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public bool AddItem(ItemsForPurchaseDTO item)
    {
        if (SelectedItems.Any(selected => selected.Id == item.Id))
            return false;

        SelectedItems.Add(item);
        NotifyStateChanged();
        return true;
    }

    public void RemoveItem(int itemId)
    {
        var selectedItem = SelectedItems.FirstOrDefault(item => item.Id == itemId);
        if (selectedItem is null)
            return;

        SelectedItems.Remove(selectedItem);
        NotifyStateChanged();
    }

    public void ClearCart()
    {
        if (SelectedItems.Count == 0)
            return;

        SelectedItems.Clear();
        NotifyStateChanged();
    }
}
