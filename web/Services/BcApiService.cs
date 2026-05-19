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

    public async Task<SyncResult> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var payload = new List<BcCustomerDto>
        {
            new()
            {
                DisplayName = request.DisplayName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            }
        };

        var response = await _httpClient.PostAsJsonAsync("PushCustomers", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SyncResult>();
        return result ?? throw new InvalidOperationException("Empty response from API.");
    }
}
