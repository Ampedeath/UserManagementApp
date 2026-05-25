using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserManagementApp.Tests.Core.Configuration;

public static class TestJsonOptionsFactory
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