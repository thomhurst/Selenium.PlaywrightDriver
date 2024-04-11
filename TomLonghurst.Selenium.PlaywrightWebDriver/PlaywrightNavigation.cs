using System;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightNavigation : INavigation
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightNavigation(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public void Back()
    {
        _playwrightWebDriver.CurrentPage.GoBackAsync().Synchronously();
    }

    public void Forward()
    {
        _playwrightWebDriver.CurrentPage.GoForwardAsync().Synchronously();
    }

    public void GoToUrl(string url)
    {
        _playwrightWebDriver.Url = url;
    }

    public void GoToUrl(Uri url)
    {
        GoToUrl(url.ToString());
    }

    public void Refresh()
    {
        _playwrightWebDriver.CurrentPage.ReloadAsync().Synchronously();
    }
}