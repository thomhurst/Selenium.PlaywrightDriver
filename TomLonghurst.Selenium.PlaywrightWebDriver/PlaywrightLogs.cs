using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightLogs : ILogs
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightLogs(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public ReadOnlyCollection<LogEntry> GetLog(string logKind)
    {
        return new(new List<LogEntry>());
    }

    public ReadOnlyCollection<string> AvailableLogTypes { get; } = new(new List<string>());
}