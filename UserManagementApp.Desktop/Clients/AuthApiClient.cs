using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Desktop.Configuration;

namespace UserManagementApp.Desktop.Clients;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = JsonOptionsFactory.Create();
    }

    public async Task<LoginResponse?> LoginAsync(string userName, string password)
    {
        var request = new LoginRequest
        {
            UserName = userName,
            Password = password
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
    }
}