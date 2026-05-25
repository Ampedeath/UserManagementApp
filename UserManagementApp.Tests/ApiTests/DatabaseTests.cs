using FluentAssertions;
using UserManagementApp.Tests.Core;
using UserManagementApp.Tests.Core.Clients;
using static UserManagementApp.Tests.Core.Logging.TestStepLogger;

namespace UserManagementApp.Tests.ApiTests;

public class DatabaseTests : BaseApiTest
{
    [Test]
    public async Task GetDatabaseStatus_WhenApiAndDatabaseAreAvailable()
    {
        DatabaseApiClient databaseApiClient = null!;
        DatabaseStatusResponse? response = null;

        TestCaseStep("Create database API client", () =>
        {
            databaseApiClient = new DatabaseApiClient(HttpClient, Log);
        });

        await TestCaseStep("Send request to get database status", async () =>
        {
            response = await databaseApiClient.GetStatusAsync();
        });

        TestCaseStep("Verify database status response", () =>
        {
            response.Should().NotBeNull();

            response!.Status.Should().Be("Connected");
            response.CanConnect.Should().BeTrue();
            response.Provider.Should().NotBeNullOrWhiteSpace();
            response.Provider.Should().Contain("Sqlite");
        });
    }
}