using System;
using System.Threading.Tasks;
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
        BackAsync().Synchronously();
    }

    public Task BackAsync()
    {
        return _playwrightWebDriver.CurrentPage.GoBackAsync();
    }

    public void Forward()
    {
        ForwardAsync().Synchronously();
    }

    public Task ForwardAsync()
    {
        return _playwrightWebDriver.CurrentPage.GoForwardAsync();
    }

    public void GoToUrl(string url)
    {
        GoToUrlAsync(url).Synchronously();
    }

    public Task GoToUrlAsync(string url)
    {
        return _playwrightWebDriver.CurrentPage.GotoAsync(url);
    }

    public void GoToUrl(Uri url)
    {
        GoToUrl(url.ToString());
    }

    public Task GoToUrlAsync(Uri url)
    {
        return GoToUrlAsync(url.ToString());
    }

    public void Refresh()
    {
        RefreshAsync().Synchronously();
    }

    public Task RefreshAsync()
    {
        return _playwrightWebDriver.CurrentPage.ReloadAsync(); 
    }
}