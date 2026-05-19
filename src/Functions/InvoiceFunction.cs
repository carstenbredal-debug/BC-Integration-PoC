using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using BCIntegrationPoC.Models;
using BCIntegrationPoC.Services;

namespace BCIntegrationPoC.Functions;

public class InvoiceFunction
{
    private readonly BusinessCentralApiClient _bcClient;
    private readonly ILogger<InvoiceFunction> _logger;

    public InvoiceFunction(
        BusinessCentralApiClient bcClient,
        ILogger<InvoiceFunction> logger)
    {
        _bcClient = bcClient;
        _logger = logger;
    }

    [Function("GetItems")]
    public async Task<HttpResponseData> GetItems(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Fetching items from Business Central");

        var companyId = await _bcClient.ResolveCompanyIdAsync();
        var items = await _bcClient.GetItemsAsync(companyId, 250);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(items));
        return response;
    }

    [Function("CreateSalesInvoice")]
    public async Task<HttpResponseData> CreateSalesInvoice(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Creating sales invoice in Business Central");

        var body = await req.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request body is required.");
            return bad;
        }

        var request = JsonSerializer.Deserialize<CreateInvoiceRequest>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (request is null || request.CustomerId == Guid.Empty)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("customerId is required.");
            return bad;
        }

        var companyId = await _bcClient.ResolveCompanyIdAsync();

        var invoice = new BcSalesInvoice
        {
            CustomerId = request.CustomerId
        };

        if (!string.IsNullOrWhiteSpace(request.ExternalDocumentNumber))
            invoice.ExternalDocumentNumber = request.ExternalDocumentNumber;

        BcSalesInvoice created;
        try
        {
            created = await _bcClient.CreateSalesInvoiceAsync(companyId, invoice);
            _logger.LogInformation("Created invoice {Number} (id={Id})", created.Number, created.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create invoice header");
            var err = req.CreateResponse(HttpStatusCode.InternalServerError);
            await err.WriteStringAsync($"Failed to create invoice: {ex.Message}");
            return err;
        }

        var lineResults = new List<object>();
        var errors = new List<string>();

        if (request.Lines is { Count: > 0 })
        {
            foreach (var line in request.Lines)
            {
                try
                {
                    var invoiceLine = new BcSalesInvoiceLine
                    {
                        LineType = "Item",
                        ItemId = line.ItemId,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice
                    };

                    if (!string.IsNullOrWhiteSpace(line.Description))
                        invoiceLine.Description = line.Description;

                    var createdLine = await _bcClient.AddSalesInvoiceLineAsync(
                        companyId, created.Id, invoiceLine);

                    lineResults.Add(new
                    {
                        createdLine.Id,
                        createdLine.LineObjectNumber,
                        createdLine.Description,
                        createdLine.Quantity,
                        createdLine.UnitPrice,
                        createdLine.NetAmount
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Line item {line.ItemId}: {ex.Message}");
                    _logger.LogError(ex, "Failed to add invoice line for item {ItemId}", line.ItemId);
                }
            }
        }

        var updatedInvoice = await _bcClient.GetSalesInvoiceAsync(companyId, created.Id);

        var result = new
        {
            invoiceId = created.Id,
            invoiceNumber = created.Number,
            customerId = created.CustomerId,
            customerName = created.CustomerName,
            status = updatedInvoice?.Status ?? created.Status,
            totalAmountExcludingTax = updatedInvoice?.TotalAmountExcludingTax ?? 0,
            totalAmountIncludingTax = updatedInvoice?.TotalAmountIncludingTax ?? 0,
            linesCreated = lineResults.Count,
            linesFailed = errors.Count,
            lines = lineResults,
            errors
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(result));
        return response;
    }
}

public class CreateInvoiceRequest
{
    public Guid CustomerId { get; set; }
    public string? ExternalDocumentNumber { get; set; }
    public List<CreateInvoiceLineRequest>? Lines { get; set; }
}

public class CreateInvoiceLineRequest
{
    public Guid ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
