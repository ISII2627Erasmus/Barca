using Newtonsoft.Json;

namespace AppForSEII.Web.API;

// The local NSwag client was generated from an older purchase schema. These
// fields are part of the API's current request and response DTOs.
public partial class PurchaseForCreateDTO
{
    [JsonProperty("customerUserName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string CustomerUserName { get; set; } = string.Empty;
}

public partial class PurchaseDetailDTO
{
    [JsonProperty("customerName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomerName { get; set; }

    [JsonProperty("customerSurname", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomerSurname { get; set; }

    [JsonProperty("customerUserName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? CustomerUserName { get; set; }
}

public partial class PurchaseItemDetailDTO
{
    [JsonProperty("sportName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? SportName { get; set; }
}
