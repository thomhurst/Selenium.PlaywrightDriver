using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightNetwork : INetwork
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightNetwork(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public void AddRequestHandler(NetworkRequestHandler handler)
    {
    }

    public void ClearRequestHandlers()
    {
    }

    public void AddAuthenticationHandler(NetworkAuthenticationHandler handler)
    {
    }

    public void ClearAuthenticationHandlers()
    {
    }

    public void AddResponseHandler(NetworkResponseHandler handler)
    {
    }

    public void ClearResponseHandlers()
    {
    }

    public Task StartMonitoring()
    {
        return Task.CompletedTask;
    }

    public Task StopMonitoring()
    {
        return Task.CompletedTask;
    }

    public event EventHandler<NetworkRequestSentEventArgs>? NetworkRequestSent;
    public event EventHandler<NetworkResponseReceivedEventArgs>? NetworkResponseReceived;
}