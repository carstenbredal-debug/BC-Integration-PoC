using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcCountryRegion
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("@odata.etag")]
    public string? ETag { get; set; }
}
