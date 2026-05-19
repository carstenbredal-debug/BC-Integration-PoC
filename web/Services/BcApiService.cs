using System.Net.Http.Json;
using web.Models;

namespace web.Services;

public class BcApiService
{
    private readonly HttpClient _httpClient;

    public BcApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CountryRegion>> GetCountriesRegionsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CountryRegion>>("GetCountriesRegions");
        return result ?? new List<CountryRegion>();
    }

    public async Task<List<PaymentTerm>> GetPaymentTermsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PaymentTerm>>("GetPaymentTerms");
        return result ?? new List<PaymentTerm>();
    }

    public async Task<List<PostingGroup>> GetGenBusPostingGroupsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PostingGroup>>("GetGenBusPostingGroups");
        return result ?? new List<PostingGroup>();
    }

    public async Task<List<PostingGroup>> GetVatBusPostingGroupsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PostingGroup>>("GetVatBusPostingGroups");
        return result ?? new List<PostingGroup>();
    }

    public async Task<SyncResult> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var dto = new BcCustomerDto
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AddressLine1 = request.AddressLine1,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            GenBusPostingGroup = request.GenBusPostingGroup,
            VatBusPostingGroup = request.VatBusPostingGroup
        };

        if (Guid.TryParse(request.PaymentTermsId, out _))
            dto.PaymentTermsId = request.PaymentTermsId;

        var payload = new List<BcCustomerDto> { dto };

        var response = await _httpClient.PostAsJsonAsync("PushCustomers", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SyncResult>();
        return result ?? throw new InvalidOperationException("Empty response from API.");
    }
}
