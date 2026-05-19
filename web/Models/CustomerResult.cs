using System.Text.Json.Serialization;

namespace web.Models;

public class SyncResult
{
    [JsonPropertyName("entity")]
    public string Entity { get; set; } = string.Empty;

    [JsonPropertyName("processed")]
    public int Processed { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("updated")]
    public int Updated { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}

public class BcCustomerDto
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("addressLine1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("paymentTermsId")]
    public string PaymentTermsId { get; set; } = string.Empty;

    [JsonPropertyName("genBusPostingGroup")]
    public string GenBusPostingGroup { get; set; } = string.Empty;

    [JsonPropertyName("vatBusPostingGroup")]
    public string VatBusPostingGroup { get; set; } = string.Empty;

    [JsonPropertyName("customerPostingGroup")]
    public string CustomerPostingGroup { get; set; } = string.Empty;
}
