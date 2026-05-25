
namespace UserManagementApp.Tests.Core.Logging;

public static class TestStepLogger
{
    public static async Task TestCaseStep(string description, Func<Task> action)
    {
        TestContext.Progress.WriteLine($"[STEP] {description}");

        try
        {
            await action();

            TestContext.Progress.WriteLine($"[STEP PASSED] {description}");
        }
        catch (Exception exception)
        {
            TestContext.Progress.WriteLine($"[STEP FAILED] {description}");
            TestContext.Progress.WriteLine(exception.Message);

            throw;
        }
    }
    
    public static void TestCaseStep(string description, Action action)
    {
        TestContext.Progress.WriteLine($"[STEP] {description}");

        try
        {
            action();

            TestContext.Progress.WriteLine($"[STEP PASSED] {description}");
        }
        catch (Exception exception)
        {
            TestContext.Progress.WriteLine($"[STEP FAILED] {description}");
            TestContext.Progress.WriteLine(exception.Message);

            throw;
        }
    }
}