using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Microsoft.Playwright;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightWebElement : IWebElement
{
    public readonly ILocator Locator;
    internal readonly string _locatorString;

    public PlaywrightWebElement(ILocator locator, string locatorString)
    {
        Locator = locator;
        _locatorString = locatorString;
    }

    public IWebElement FindElement(By by)
    {
        var locatorString = LocatorHelpers.GetLocatorString(by);

        var locator = Locator.Locator(locatorString).First;

        return new PlaywrightWebElement(locator, locatorString);
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        var locatorString = LocatorHelpers.GetLocatorString(by);

        var locator = Locator.Locator(locatorString);

        var playwrightWebElements = locator.AllAsync()
            .Synchronously()
            .Select(x => new PlaywrightWebElement(x, locatorString))
            .Cast<IWebElement>()
            .ToList();
            
        return new ReadOnlyCollection<IWebElement>(playwrightWebElements);
    }

    public void Clear()
    {
        Locator.ClearAsync().Synchronously();
    }

    public void SendKeys(string text)
    {
        if (GetProperty("type") == "file")
        {
            Locator.SetInputFilesAsync(text).Synchronously();
            return;
        }
        
        Locator.FillAsync(text).Synchronously();
    }

    public void Submit()
    {
        Locator.DispatchEventAsync("submit").Synchronously();
    }

    public void Click()
    {
        Locator.ClickAsync().Synchronously();
    }

    public string? GetAttribute(string attributeName)
    {
        return Locator.GetAttributeAsync(attributeName).Synchronously();
    }

    public string? GetProperty(string propertyName)
    {
        return GetAttribute(propertyName);
    }

    public string? GetDomAttribute(string attributeName)
    {
        return GetAttribute(attributeName);
    }

    public string? GetDomProperty(string propertyName)
    {
        return GetAttribute(propertyName);
    }

    public string GetCssValue(string propertyName)
    {
        return Locator
            .EvaluateAsync<string>($"(element) => window.getComputedStyle(element).getPropertyValue('{propertyName}')")
            .Synchronously();
    }

    public ISearchContext GetShadowRoot()
    {
        return this;
    }

    public string TagName => Locator.EvaluateAsync<string>("(element) => element.tagName").Synchronously();
    public string Text => Locator.TextContentAsync().Synchronously() ?? string.Empty;
    public bool Enabled => Locator.IsEnabledAsync().Synchronously();
    public bool Selected => Locator.IsCheckedAsync().Synchronously();
    public Point Location
    {
        get
        {
            var boundingBox = Locator.BoundingBoxAsync().Synchronously() ?? new LocatorBoundingBoxResult();
            return new Point((int)boundingBox.X, (int)boundingBox.Y);
        }
    }

    public Size Size
    {
        get
        {
            var boundingBox = Locator.BoundingBoxAsync().Synchronously() ?? new LocatorBoundingBoxResult();
            return new Size((int)boundingBox.Width, (int)boundingBox.Height);
        }
    }

    public bool Displayed => Locator.IsVisibleAsync().Synchronously();
}