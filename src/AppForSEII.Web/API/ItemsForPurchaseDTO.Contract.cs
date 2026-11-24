using Newtonsoft.Json;

namespace AppForSEII.Web.API;

// These properties match the current API's ItemsForPurchaseDTO response. The
// generated client model predates the API's move to a flat SportName field.
public partial class ItemsForPurchaseDTO
{
    [JsonProperty("sportName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? SportName { get; set; }

    [JsonProperty("quantityAvailableForPurchase", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int QuantityAvailableForPurchase { get; set; }
}
