using Microsoft.Extensions.Configuration;

namespace UserManagementApp.Desktop.Configuration;

public static class ConfigurationFactory
{
    public static IConfigurationRoot CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                path: "appsettings.json",
                optional: false,
                reloadOnChange: true)
            .Build();
    }
}