using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Tests.Core.Configuration;
using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core.Clients;

public class UsersApiClient
{
    private readonly Action<int>? _registerUserForCleanup;
    private readonly HttpClient _httpClient;
    private readonly TestLog _log;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsersApiClient(HttpClient httpClient, TestLog log, Action<int>? registerUserForCleanup = null)
    {
        _httpClient = httpClient;
        _log = log;
        _jsonOptions = TestJsonOptionsFactory.Create();
        _registerUserForCleanup = registerUserForCleanup;
    }

    public async Task<ApiTestResponse<List<UserResponse>>> GetUsersAsync(int currentUserId)
    {
        const string url = "/api/users";

        using var request = CreateRequest(HttpMethod.Get, url, currentUserId);

        _log.Request(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        List<UserResponse>? body = null;

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(rawBody))
        {
            body = JsonSerializer.Deserialize<List<UserResponse>>(rawBody, _jsonOptions);
        }

        return new ApiTestResponse<List<UserResponse>>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = body
        };
    }

    public async Task<ApiTestResponse<UserResponse>> GetUserByIdAsync(int userId, int currentUserId)
    {
        var url = $"/api/users/{userId}";

        using var request = CreateRequest(HttpMethod.Get, url, currentUserId);

        _log.Request(HttpMethod.Get, url);

        var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        UserResponse? body = null;

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(rawBody))
        {
            body = JsonSerializer.Deserialize<UserResponse>(rawBody, _jsonOptions);
        }

        return new ApiTestResponse<UserResponse>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = body
        };
    }

    public async Task<ApiTestResponse<UserResponse>> CreateUserAsync(CreateUserRequest createUserRequest, int currentUserId)
    {
        const string url = "/api/users";

        using var request = CreateRequest(HttpMethod.Post, url, currentUserId);
        request.Content = JsonContent.Create(createUserRequest, options: _jsonOptions);

        _log.Request(HttpMethod.Post, url);

        var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        UserResponse? body = null;

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(rawBody))
        {
            body = JsonSerializer.Deserialize<UserResponse>(rawBody, _jsonOptions);
        }
        
        if (response.StatusCode == HttpStatusCode.Created && body is not null)
        {
            _registerUserForCleanup?.Invoke(body.UserId);
        }

        return new ApiTestResponse<UserResponse>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = body
        };
    }

    public async Task<ApiTestResponse<UserResponse>> UpdateUserAsync(
        int userId,
        UpdateUserRequest updateUserRequest,
        int currentUserId)
    {
        var url = $"/api/users/{userId}";

        using var request = CreateRequest(HttpMethod.Put, url, currentUserId);
        request.Content = JsonContent.Create(updateUserRequest, options: _jsonOptions);

        _log.Request(HttpMethod.Put, url);

        var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        UserResponse? body = null;

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(rawBody))
        {
            body = JsonSerializer.Deserialize<UserResponse>(rawBody, _jsonOptions);
        }

        return new ApiTestResponse<UserResponse>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = body
        };
    }

    public async Task<ApiTestResponse<object>> DeleteUserAsync(int userId, int currentUserId)
    {
        var url = $"/api/users/{userId}";

        using var request = CreateRequest(HttpMethod.Delete, url, currentUserId);

        _log.Request(HttpMethod.Delete, url);

        var response = await _httpClient.SendAsync(request);
        var rawBody = await response.Content.ReadAsStringAsync();

        _log.Response(response, rawBody);

        return new ApiTestResponse<object>
        {
            StatusCode = response.StatusCode,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            RawBody = rawBody,
            Body = null
        };
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, int currentUserId)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Add("X-User-Id", currentUserId.ToString());

        return request;
    }
}