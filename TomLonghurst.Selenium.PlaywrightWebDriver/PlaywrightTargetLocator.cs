using System;
using System.Linq;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightTargetLocator : ITargetLocator
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightTargetLocator(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public IWebDriver Frame(int frameIndex)
    {
        var frames = _playwrightWebDriver.CurrentPage.Frames;

        var frame = frames.ElementAtOrDefault(frameIndex);
        
        if (frame is null)
        {
            throw new NoSuchFrameException($"No frame found at index {frameIndex}");
        }
        
        return _playwrightWebDriver.SetFrame(frame.FrameLocator(":root"));
    }

    public IWebDriver Frame(string frameName)
    {
        var locatorString = LocatorHelpers.GetLocatorString(By.Name(frameName));
        var frame = _playwrightWebDriver.CurrentFrameLocators.Any()
            ? _playwrightWebDriver.CurrentFrameLocators.Last().FrameLocator(locatorString)
            : _playwrightWebDriver.CurrentPage.FrameLocator(locatorString);
        
        if (frame.Owner.CountAsync().Synchronously() == 0)
        {
            throw new NoSuchFrameException($"No frame found named {frameName}");
        }
        
        return _playwrightWebDriver.SetFrame(frame);
    }

    public IWebDriver Frame(IWebElement frameElement)
    {
        if (frameElement is not PlaywrightWebElement playwrightWebElement)
        {
            throw new ArgumentException("Not a PlaywrightWebElement");
        }

        if (frameElement.TagName is not "IFRAME" and not "FRAME")
        {
            throw new NoSuchFrameException($"Not an iFrame: {frameElement.TagName}");
        }
        
        var frameLocator = _playwrightWebDriver.CurrentPage.FrameLocator(playwrightWebElement._locatorString);
        
        if (frameLocator.Owner.CountAsync().Synchronously() == 0)
        {
            throw new NoSuchFrameException($"No frame found: {playwrightWebElement._locatorString}");
        }
        
        return _playwrightWebDriver.SetFrame(frameLocator);    
    }

    public IWebDriver ParentFrame()
    {
        var toPop = _playwrightWebDriver.CurrentFrameLocators.LastOrDefault();
        
        if (toPop is not null)
        {
            _playwrightWebDriver.CurrentFrameLocators.Remove(toPop);
        }

        if (_playwrightWebDriver.CurrentFrameLocators.LastOrDefault() is { } lastFrame)
        {
            _playwrightWebDriver.SetFrame(lastFrame);
        }
        else
        {
            _playwrightWebDriver.CurrentFrameLocators.Clear();
        }

        return _playwrightWebDriver;
    }

    public IWebDriver Window(string windowName)
    {
        if(!_playwrightWebDriver.KeyToPage.TryGetValue(windowName, out var window))
        {
            throw new NoSuchWindowException($"No window with handle: {windowName}");
        }

        _playwrightWebDriver.CurrentPage = window;
        return _playwrightWebDriver;
    }

    public IWebDriver NewWindow(WindowType typeHint)
    {
        _playwrightWebDriver.NewPage();
        return _playwrightWebDriver;
    }

    public IWebDriver DefaultContent()
    {
        _playwrightWebDriver.CurrentPage = _playwrightWebDriver.OriginalPage;
        return _playwrightWebDriver;
    }

    public IWebElement ActiveElement()
    {
        return _playwrightWebDriver.FindElement(PlaywrightBy.Selector("*:focus"));
    }

    public IAlert Alert()
    {
        return new PlaywrightAlert(_playwrightWebDriver);
    }
}