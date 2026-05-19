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
}
