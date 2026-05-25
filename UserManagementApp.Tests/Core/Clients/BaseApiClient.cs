using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApp.Tests.Core.Configuration;
using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core.Clients;

public abstract class BaseApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TestLog _log;
    private readonly JsonSerializerOptions _jsonOptions;

    protected BaseApiClient(HttpClient httpClient, TestLog log)
    {
        _httpClient = httpClient;
        _log = log;
        _jsonOptions = TestJsonOptionsFactory.Create();
    }

    protected async Task<TResponse?> GetAsync<TResponse>(string url)
    {
        _log.Request(HttpMethod.Get, url);

        var response = await _httpClient.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();

        _log.Response(response, body);

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url,
        TRequest requestBody)
    {
        _log.Request(HttpMethod.Post, url);

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, _jsonOptions);
        var body = await response.Content.ReadAsStringAsync();

        _log.Response(response, body);

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
    }
}