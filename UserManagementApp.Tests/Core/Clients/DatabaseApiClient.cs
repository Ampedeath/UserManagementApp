using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core.Clients;

public class DatabaseApiClient : BaseApiClient
{
    public DatabaseApiClient(HttpClient httpClient, TestLog log) : base(httpClient, log)
    {
    }

    public async Task<DatabaseStatusResponse?> GetStatusAsync()
    {
        return await GetAsync<DatabaseStatusResponse>("/api/database/status");
    }
}

public class DatabaseStatusResponse
{
    public string Status { get; set; } = string.Empty;

    public bool CanConnect { get; set; }

    public string? Provider { get; set; }
}