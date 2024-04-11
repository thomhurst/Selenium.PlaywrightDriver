using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightAlert : IAlert
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    internal PlaywrightAlert(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public void Dismiss()
    {
    }

    public void Accept()
    {
    }

    public void SendKeys(string keysToSend)
    {
        throw new System.NotImplementedException();
    }

    public string? Text => _playwrightWebDriver._lastAlertText;
}