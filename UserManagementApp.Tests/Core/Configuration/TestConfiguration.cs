using Microsoft.Extensions.Configuration;

namespace UserManagementApp.Tests.Core.Configuration;

public static class TestConfiguration
{
    private static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
    }

    public static ApiTestSettings LoadApiSettings()
    {
        var configuration = LoadConfiguration();

        var settings = configuration
            .GetSection("ApiSettings")
            .Get<ApiTestSettings>();

        if (settings is null || string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        }

        return settings;
    }

    public static DatabaseTestSettings LoadDatabaseSettings()
    {
        var configuration = LoadConfiguration();

        var settings = configuration
            .GetSection("DatabaseSettings")
            .Get<DatabaseTestSettings>();

        if (settings is null || string.IsNullOrWhiteSpace(settings.DatabaseFilePath))
        {
            throw new InvalidOperationException("DatabaseSettings:DatabaseFilePath is not configured.");
        }

        return settings;
    }
    
    public static string BuildDatabaseConnectionString()
    {
        var settings = LoadDatabaseSettings();

        var databasePath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, settings.DatabaseFilePath));

        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException($"Database file was not found. Expected path: {databasePath}");
        }

        return $"Data Source={databasePath}";
    }
}