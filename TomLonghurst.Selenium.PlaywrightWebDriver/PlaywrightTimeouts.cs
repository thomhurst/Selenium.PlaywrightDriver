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

    public ITimeouts ImplicitlyWait(TimeSpan timeToWait)
    {
        ImplicitWait = timeToWait;
        return this;
    }

    public ITimeouts SetScriptTimeout(TimeSpan timeToWait)
    {
        AsynchronousJavaScript = timeToWait;
        return this;
    }

    public ITimeouts SetPageLoadTimeout(TimeSpan timeToWait)
    {
        PageLoad = timeToWait;
        return this;
    }
}