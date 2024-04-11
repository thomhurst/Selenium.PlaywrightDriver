using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightBy : By
{
    public string PlaywrightLocator { get; }

    private PlaywrightBy(string playwrightLocator)
    {
        PlaywrightLocator = playwrightLocator;
    }

    public static PlaywrightBy Selector(string playwrightLocator) => new(playwrightLocator);
}