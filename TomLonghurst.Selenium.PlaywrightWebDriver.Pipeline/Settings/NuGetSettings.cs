namespace TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline.Settings;

public record NuGetSettings
{
    public string? ApiKey { get; init; }
    public bool ShouldPublish { get; set; }
}
