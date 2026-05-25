
namespace UserManagementApp.Tests.Core.Logging;

public class TestLog
{
    public void Info(string message)
    {
        TestContext.Progress.WriteLine($"[INFO] {message}");
    }

    public void Request(HttpMethod method, string url)
    {
        TestContext.Progress.WriteLine($"[REQUEST] {method} {url}");
    }

    public void Response(HttpResponseMessage response, string? body = null)
    {
        TestContext.Progress.WriteLine($"[RESPONSE] {(int)response.StatusCode} {response.StatusCode}");

        if (!string.IsNullOrWhiteSpace(body))
        {
            TestContext.Progress.WriteLine("[BODY]");
            TestContext.Progress.WriteLine(body);
        }
    }
}