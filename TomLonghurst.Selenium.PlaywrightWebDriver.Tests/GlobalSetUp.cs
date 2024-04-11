using System.Diagnostics;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Tests;

[SetUpFixture]
public static class GlobalSetUp
{
    [OneTimeSetUp]
    public static async Task PlaywrightSetup()
    {
        await Process.Start("pwsh", new[] { "playwright.ps1", "install" }).WaitForExitAsync();
    }
}