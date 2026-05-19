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

    public async Task<SyncResult> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var payload = new List<BcCustomerDto>
        {
            new()
            {
                DisplayName = request.DisplayName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country
            }
        };

        var response = await _httpClient.PostAsJsonAsync("PushCustomers", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SyncResult>();
        return result ?? throw new InvalidOperationException("Empty response from API.");
    }
}
