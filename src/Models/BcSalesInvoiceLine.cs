using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcSalesInvoiceLine
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("documentId")]
    public Guid DocumentId { get; set; }

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("itemId")]
    public Guid ItemId { get; set; }

    [JsonPropertyName("lineType")]
    public string LineType { get; set; } = "Item";

    [JsonPropertyName("lineObjectNumber")]
    public string LineObjectNumber { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("discountPercent")]
    public decimal DiscountPercent { get; set; }

    [JsonPropertyName("netAmount")]
    public decimal NetAmount { get; set; }

    [JsonPropertyName("netAmountIncludingTax")]
    public decimal NetAmountIncludingTax { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string? ETag { get; set; }
}
