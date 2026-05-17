using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApp.Core.DTOs.Users;
using UserManagementApp.Desktop.Configuration;

namespace UserManagementApp.Desktop.Clients;

public class UserApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = JsonOptionsFactory.Create();
    }

     public async Task<List<UserResponse>> GetUsersAsync(int currentUserId)
    {
        using var request = CreateRequest(HttpMethod.Get, "/api/users", currentUserId);

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(_jsonOptions);

        return users ?? new List<UserResponse>();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId, int currentUserId)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/users/{userId}", currentUserId);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserResponse>(_jsonOptions);
    }

    public async Task<UserResponse> CreateUserAsync(
        CreateUserRequest createUserRequest,
        int currentUserId)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/users", currentUserId);

        request.Content = JsonContent.Create(
            createUserRequest,
            options: _jsonOptions);

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var createdUser = await response.Content.ReadFromJsonAsync<UserResponse>(_jsonOptions);

        return createdUser
               ?? throw new InvalidOperationException("API returned empty response when creating user.");
    }

    public async Task<UserResponse?> UpdateUserAsync(
        int userId,
        UpdateUserRequest updateUserRequest,
        int currentUserId)
    {
        using var request = CreateRequest(HttpMethod.Put, $"/api/users/{userId}", currentUserId);

        request.Content = JsonContent.Create(
            updateUserRequest,
            options: _jsonOptions);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserResponse>(_jsonOptions);
    }

    public async Task<bool> DeleteUserAsync(int userId, int currentUserId)
    {
        using var request = CreateRequest(HttpMethod.Delete, $"/api/users/{userId}", currentUserId);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
    
    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        int currentUserId)
    {
        var request = new HttpRequestMessage(method, url);

        request.Headers.Add("X-User-Id", currentUserId.ToString());

        return request;
    }
}