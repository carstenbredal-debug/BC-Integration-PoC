using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcPostingGroup
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}
