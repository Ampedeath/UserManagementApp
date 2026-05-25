using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApp.Core.DTOs.Auth;
using UserManagementApp.Tests.Core.Configuration;
using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core.Clients;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TestLog _log;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiClient(HttpClient httpClient, TestLog log)
    {
        _httpClient = httpClient;
        _log = log;
        _jsonOptions = TestJsonOptionsFactory.Create();
    }

    public async Task<ApiTestResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        _log.Request(HttpMethod.Post, "/api/auth/login");

        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request, _jsonOptions);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        LoginResponse? body = null;

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(rawBody))
        {
            body = JsonSerializer.Deserialize<LoginResponse>(rawBody, _jsonOptions);
        }

        return new ApiTestResponse<LoginResponse>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = body
        };
    }
}