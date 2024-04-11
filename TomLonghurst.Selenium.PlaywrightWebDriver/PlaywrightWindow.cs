using System.Drawing;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightWindow : IWindow
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightWindow(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public void Maximize()
    {
        Size = new Size(1080, 1920);
    }

    public void Minimize()
    {
    }

    public void FullScreen()
    {
        Maximize();
    }

    public Point Position
    {
        get => new(0, 0);
        set
        {
        }
    }

    public Size Size
    {
        get => new(_playwrightWebDriver.CurrentPage.ViewportSize?.Width ?? 0, _playwrightWebDriver.CurrentPage.ViewportSize?.Height ?? 0);
        set => _playwrightWebDriver.CurrentPage.SetViewportSizeAsync(value.Width, value.Height).Synchronously();
    }
}