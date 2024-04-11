using System;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightTimeouts : ITimeouts
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightTimeouts(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public TimeSpan ImplicitWait
    {
        get => TimeSpan.FromSeconds(30);
        set => _playwrightWebDriver.CurrentPage.SetDefaultTimeout((float)value.TotalMilliseconds);
    }

    public TimeSpan AsynchronousJavaScript
    {
        get => TimeSpan.FromSeconds(30);
        set => _playwrightWebDriver.CurrentPage.SetDefaultTimeout((float)value.TotalMilliseconds);
    }
    
    public TimeSpan PageLoad
    {
        get => TimeSpan.FromSeconds(30);
        set => _playwrightWebDriver.CurrentPage.SetDefaultNavigationTimeout((float)value.TotalMilliseconds);
    }
}