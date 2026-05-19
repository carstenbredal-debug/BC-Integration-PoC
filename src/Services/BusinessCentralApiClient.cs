using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BCIntegrationPoC.Configuration;
using BCIntegrationPoC.Models;

namespace BCIntegrationPoC.Services;

public class BusinessCentralApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BusinessCentralAuthService _authService;
    private readonly BusinessCentralOptions _options;
    private readonly ILogger<BusinessCentralApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BusinessCentralApiClient(
        HttpClient httpClient,
        BusinessCentralAuthService authService,
        IOptions<BusinessCentralOptions> options,
        ILogger<BusinessCentralApiClient> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _options = options.Value;
        _logger = logger;
    }

    // ── Companies ──────────────────────────────────────────────

    public async Task<List<BcCompany>> GetCompaniesAsync()
    {
        var url = $"{_options.BaseUrl}/companies";
        return await GetListAsync<BcCompany>(url);
    }

    public async Task<Guid> ResolveCompanyIdAsync()
    {
        if (Guid.TryParse(_options.CompanyId, out var configured))
            return configured;

        var companies = await GetCompaniesAsync();
        if (companies.Count == 0)
            throw new InvalidOperationException("No companies found in Business Central.");

        var company = companies[0];
        _logger.LogInformation("Using company '{Name}' ({Id})", company.DisplayName, company.Id);
        return company.Id;
    }

    public async Task<string> ResolveCompanyNameAsync()
    {
        var companyId = await ResolveCompanyIdAsync();
        var companies = await GetCompaniesAsync();
        var company = companies.FirstOrDefault(c => c.Id == companyId)
            ?? throw new InvalidOperationException($"Company {companyId} not found.");
        return company.Name;
    }

    // ── Countries/Regions ───────────────────────────────────────

    public async Task<List<BcCountryRegion>> GetCountriesRegionsAsync(Guid companyId)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/countriesRegions?$top=250&$orderby=displayName";
        return await GetListAsync<BcCountryRegion>(url);
    }

    // ── Payment Terms ────────────────────────────────────────────

    public async Task<List<BcPaymentTerm>> GetPaymentTermsAsync(Guid companyId)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/paymentTerms?$top=250&$orderby=displayName";
        return await GetListAsync<BcPaymentTerm>(url);
    }

    // ── Posting Groups (via OData v4 workflowCustomers) ─────────

    public async Task<List<BcPostingGroup>> GetGenBusPostingGroupsAsync(string companyName)
    {
        var url = $"{_options.ODataV4Url}/Company('{Uri.EscapeDataString(companyName)}')/workflowCustomers?$select=genBusPostingGroup&$top=500";
        var customers = await GetListAsync<BcWorkflowCustomer>(url);
        return customers
            .Select(c => c.GenBusPostingGroup)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .Select(g => new BcPostingGroup { Code = g })
            .ToList();
    }

    public async Task<List<BcPostingGroup>> GetVatBusPostingGroupsAsync(string companyName)
    {
        var url = $"{_options.ODataV4Url}/Company('{Uri.EscapeDataString(companyName)}')/workflowCustomers?$select=vatBusPostingGroup&$top=500";
        var customers = await GetListAsync<BcWorkflowCustomer>(url);
        return customers
            .Select(c => c.VatBusPostingGroup)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .Select(g => new BcPostingGroup { Code = g })
            .ToList();
    }

    public async Task<List<BcPostingGroup>> GetCustomerPostingGroupsAsync(string companyName)
    {
        var url = $"{_options.ODataV4Url}/Company('{Uri.EscapeDataString(companyName)}')/workflowCustomers?$select=customerPostingGroup&$top=500";
        var customers = await GetListAsync<BcWorkflowCustomer>(url);
        return customers
            .Select(c => c.CustomerPostingGroup)
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Distinct()
            .OrderBy(g => g)
            .Select(g => new BcPostingGroup { Code = g })
            .ToList();
    }

    public async Task PatchCustomerPostingGroupsAsync(
        string companyName, Guid customerId, string? genBusPostingGroup, string? vatBusPostingGroup, string? customerPostingGroup = null)
    {
        if (string.IsNullOrWhiteSpace(genBusPostingGroup) && string.IsNullOrWhiteSpace(vatBusPostingGroup) && string.IsNullOrWhiteSpace(customerPostingGroup))
            return;

        var filterUrl = $"{_options.ODataV4Url}/Company('{Uri.EscapeDataString(companyName)}')/workflowCustomers?$filter=id eq {customerId}&$select=id";
        var matches = await GetListAsync<BcWorkflowCustomer>(filterUrl);
        if (matches.Count == 0)
        {
            _logger.LogWarning("Customer {Id} not found in workflowCustomers for posting group update", customerId);
            return;
        }

        var wfCustomer = matches[0];
        var patchUrl = $"{_options.ODataV4Url}/Company('{Uri.EscapeDataString(companyName)}')/workflowCustomers({customerId})";

        var patchBody = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(genBusPostingGroup))
            patchBody["genBusPostingGroup"] = genBusPostingGroup;
        if (!string.IsNullOrWhiteSpace(vatBusPostingGroup))
            patchBody["vatBusPostingGroup"] = vatBusPostingGroup;
        if (!string.IsNullOrWhiteSpace(customerPostingGroup))
            patchBody["customerPostingGroup"] = customerPostingGroup;

        await PatchRawAsync(patchUrl, patchBody, wfCustomer.ETag);
    }

    // ── Customers ──────────────────────────────────────────────

    public async Task<List<BcCustomer>> GetCustomersAsync(Guid companyId, int top = 100)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/customers?$top={top}";
        return await GetListAsync<BcCustomer>(url);
    }

    public async Task<BcCustomer?> GetCustomerAsync(Guid companyId, Guid customerId)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/customers({customerId})";
        return await GetSingleAsync<BcCustomer>(url);
    }

    public async Task<BcCustomer?> GetCustomerByNumberAsync(Guid companyId, string number)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/customers?$filter=number eq '{number}'";
        var items = await GetListAsync<BcCustomer>(url);
        return items.FirstOrDefault();
    }

    public async Task<BcCustomer> CreateCustomerAsync(Guid companyId, BcCustomer customer)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/customers";
        return await PostAsync<BcCustomer>(url, customer);
    }

    public async Task<BcCustomer> UpdateCustomerAsync(Guid companyId, BcCustomer customer)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/customers({customer.Id})";
        return await PatchAsync<BcCustomer>(url, customer, customer.ETag);
    }

    // ── Sales Invoices ───────────────────────────────────────────

    public async Task<BcSalesInvoice> CreateSalesInvoiceAsync(Guid companyId, BcSalesInvoice invoice)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/salesInvoices";
        return await PostAsync<BcSalesInvoice>(url, invoice);
    }

    public async Task<BcSalesInvoiceLine> AddSalesInvoiceLineAsync(
        Guid companyId, Guid invoiceId, BcSalesInvoiceLine line)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/salesInvoices({invoiceId})/salesInvoiceLines";
        return await PostAsync<BcSalesInvoiceLine>(url, line);
    }

    public async Task<BcSalesInvoice?> GetSalesInvoiceAsync(Guid companyId, Guid invoiceId)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/salesInvoices({invoiceId})";
        return await GetSingleAsync<BcSalesInvoice>(url);
    }

    // ── Items ──────────────────────────────────────────────────

    public async Task<List<BcItem>> GetItemsAsync(Guid companyId, int top = 100)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/items?$top={top}";
        return await GetListAsync<BcItem>(url);
    }

    public async Task<BcItem?> GetItemAsync(Guid companyId, Guid itemId)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/items({itemId})";
        return await GetSingleAsync<BcItem>(url);
    }

    public async Task<BcItem?> GetItemByNumberAsync(Guid companyId, string number)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/items?$filter=number eq '{number}'";
        var items = await GetListAsync<BcItem>(url);
        return items.FirstOrDefault();
    }

    public async Task<BcItem> CreateItemAsync(Guid companyId, BcItem item)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/items";
        return await PostAsync<BcItem>(url, item);
    }

    public async Task<BcItem> UpdateItemAsync(Guid companyId, BcItem item)
    {
        var url = $"{_options.BaseUrl}/companies({companyId})/items({item.Id})";
        return await PatchAsync<BcItem>(url, item, item.ETag);
    }

    // ── HTTP helpers ───────────────────────────────────────────

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        await SetAuthHeaderAsync();
        _logger.LogInformation("GET {Url}", url);

        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response);

        var odata = await response.Content.ReadFromJsonAsync<ODataResponse<T>>(JsonOptions);
        return odata?.Value ?? new List<T>();
    }

    private async Task<T?> GetSingleAsync<T>(string url) where T : class
    {
        await SetAuthHeaderAsync();
        _logger.LogInformation("GET {Url}", url);

        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task<T> PostAsync<T>(string url, T payload)
    {
        await SetAuthHeaderAsync();
        _logger.LogInformation("POST {Url}", url);

        var response = await _httpClient.PostAsJsonAsync(url, payload, JsonOptions);
        await EnsureSuccessAsync(response);

        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    private async Task PatchRawAsync(string url, object payload, string? etag)
    {
        await SetAuthHeaderAsync();
        _logger.LogInformation("PATCH {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        if (!string.IsNullOrEmpty(etag))
            request.Headers.Add("If-Match", etag);

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    private async Task<T> PatchAsync<T>(string url, T payload, string? etag)
    {
        await SetAuthHeaderAsync();
        _logger.LogInformation("PATCH {Url}", url);

        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        if (!string.IsNullOrEmpty(etag))
            request.Headers.Add("If-Match", etag);

        var response = await _httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);

        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError("BC API error {Status}: {Body}", (int)response.StatusCode, body);
            throw new HttpRequestException(
                $"Response status code does not indicate success: " +
                $"{(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}");
        }
    }
}
