using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcCustomer
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Company";

    [JsonPropertyName("addressLine1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonPropertyName("addressLine2")]
    public string AddressLine2 { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("website")]
    public string Website { get; set; } = string.Empty;

    [JsonPropertyName("taxLiable")]
    public bool TaxLiable { get; set; }

    [JsonPropertyName("paymentTermsId")]
    public Guid PaymentTermsId { get; set; }

    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("lastModifiedDateTime")]
    public DateTimeOffset? LastModifiedDateTime { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string? ETag { get; set; }

    // Posting group fields — not part of v2.0 API, set via OData v4 after creation.
    // Nullable so WhenWritingNull skips them when serializing to the v2.0 API.
    [JsonPropertyName("genBusPostingGroup")]
    public string? GenBusPostingGroup { get; set; }

    [JsonPropertyName("vatBusPostingGroup")]
    public string? VatBusPostingGroup { get; set; }
}
