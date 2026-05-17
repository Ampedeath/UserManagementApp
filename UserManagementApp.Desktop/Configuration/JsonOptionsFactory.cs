using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserManagementApp.Desktop.Configuration;

public static class JsonOptionsFactory
{
    public static JsonSerializerOptions Create()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }
}