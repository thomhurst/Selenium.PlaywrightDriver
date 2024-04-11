using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightOptions : IOptions
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightOptions(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }
    
    public ITimeouts Timeouts()
    {
        return new PlaywrightTimeouts(_playwrightWebDriver);
    }

    public ICookieJar Cookies => new PlaywrightCookieJar(_playwrightWebDriver);
    public IWindow Window => new PlaywrightWindow(_playwrightWebDriver);
    public ILogs Logs => new PlaywrightLogs(_playwrightWebDriver);
    
#if SeleniumVersion_4    
    public INetwork Network => new PlaywrightNetwork(_playwrightWebDriver);
#endif
}