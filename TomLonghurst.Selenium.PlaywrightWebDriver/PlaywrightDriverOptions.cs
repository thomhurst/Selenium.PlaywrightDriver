using Microsoft.Playwright;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public record PlaywrightDriverOptions
{
    public bool Headless { get; set; } = true;
    public string BrowserType { get; set; } = Microsoft.Playwright.BrowserType.Chromium;
};