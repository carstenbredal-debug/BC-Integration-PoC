using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace web.Models;

public class CreateInvoiceFormModel
{
    [Required(ErrorMessage = "Please select a customer.")]
    public string CustomerId { get; set; } = string.Empty;

    public string? ExternalDocumentNumber { get; set; }

    public List<InvoiceLineFormModel> Lines { get; set; } = new() { new() };
}

public class InvoiceLineFormModel
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class CustomerListItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}

public class ItemListItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}

public class CreateInvoiceDto
{
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("externalDocumentNumber")]
    public string? ExternalDocumentNumber { get; set; }

    [JsonPropertyName("lines")]
    public List<InvoiceLineDto>? Lines { get; set; }
}

public class InvoiceLineDto
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}

public class InvoiceResult
{
    [JsonPropertyName("invoiceId")]
    public string InvoiceId { get; set; } = string.Empty;

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("totalAmountExcludingTax")]
    public decimal TotalAmountExcludingTax { get; set; }

    [JsonPropertyName("totalAmountIncludingTax")]
    public decimal TotalAmountIncludingTax { get; set; }

    [JsonPropertyName("linesCreated")]
    public int LinesCreated { get; set; }

    [JsonPropertyName("linesFailed")]
    public int LinesFailed { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}
