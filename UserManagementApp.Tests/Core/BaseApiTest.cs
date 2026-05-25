using UserManagementApp.Tests.Core.Clients;
using UserManagementApp.Tests.Core.Configuration;
using UserManagementApp.Tests.Core.Helpers;
using UserManagementApp.Tests.Core.Logging;

namespace UserManagementApp.Tests.Core;

public abstract class BaseApiTest
{
    private readonly List<int> _userIdsToDelete = new();

    protected HttpClient HttpClient { get; private set; } = null!;

    protected TestLog Log { get; private set; } = null!;    

    protected TestDataHelper TestData { get; private set; } = null!;

    protected UsersApiClient UsersApiClient { get; private set; } = null!;

    protected AuthApiClient AuthApiClient { get; private set; } = null!;

    [SetUp]
    public void SetUp()
    {
        _userIdsToDelete.Clear();

        var apiSettings = TestConfiguration.LoadApiSettings();

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(apiSettings.BaseUrl)
        };

        Log = new TestLog();

        TestData = new TestDataHelper(Log, RegisterUserForCleanup);

        UsersApiClient = new UsersApiClient(
            HttpClient,
            Log,
            RegisterUserForCleanup);

        AuthApiClient = new AuthApiClient(HttpClient, Log);
    }

    [TearDown]
    public async Task TearDown()
    {
        await DeleteTrackedUsersAsync();

        HttpClient.Dispose();
        _userIdsToDelete.Clear();
    }

    protected void RegisterUserForCleanup(int userId)
    {
        if (!_userIdsToDelete.Contains(userId))
        {
            _userIdsToDelete.Add(userId);
            Log.Info($"User registered for cleanup: {userId}");
        }
    }

    private async Task DeleteTrackedUsersAsync()
    {
        if (!_userIdsToDelete.Any())
        {
            return;
        }

        var adminUser = await TestData.EnsureAdminUserExistsAsync();

        var cleanupClient = new UsersApiClient(HttpClient, Log);

        foreach (var userId in _userIdsToDelete.Distinct().Reverse())
        {
            try
            {
                var response = await cleanupClient.DeleteUserAsync(userId, adminUser.UserId);

                Log.Info($"Cleanup user {userId}: {response.StatusCode}");
            }
            catch (Exception exception)
            {
                Log.Info($"Cleanup failed for user {userId}: {exception.Message}");
            }
        }
    }
}