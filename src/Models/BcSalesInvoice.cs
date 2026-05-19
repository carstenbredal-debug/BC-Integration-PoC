using System.Text.Json.Serialization;

namespace BCIntegrationPoC.Models;

public class BcSalesInvoice
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("externalDocumentNumber")]
    public string? ExternalDocumentNumber { get; set; }

    [JsonPropertyName("invoiceDate")]
    public string? InvoiceDate { get; set; }

    [JsonPropertyName("postingDate")]
    public string? PostingDate { get; set; }

    [JsonPropertyName("dueDate")]
    public string? DueDate { get; set; }

    [JsonPropertyName("customerId")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("customerNumber")]
    public string? CustomerNumber { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("paymentTermsId")]
    public Guid PaymentTermsId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("totalAmountIncludingTax")]
    public decimal TotalAmountIncludingTax { get; set; }

    [JsonPropertyName("totalAmountExcludingTax")]
    public decimal TotalAmountExcludingTax { get; set; }

    [JsonPropertyName("lastModifiedDateTime")]
    public DateTimeOffset? LastModifiedDateTime { get; set; }

    [JsonPropertyName("@odata.etag")]
    public string? ETag { get; set; }
}
