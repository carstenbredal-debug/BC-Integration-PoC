using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcWorkflowCustomer
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("genBusPostingGroup")]
    public string GenBusPostingGroup { get; set; } = string.Empty;

    [JsonPropertyName("vatBusPostingGroup")]
    public string VatBusPostingGroup { get; set; } = string.Empty;

    [JsonPropertyName("@odata.etag")]
    public string? ETag { get; set; }
}
