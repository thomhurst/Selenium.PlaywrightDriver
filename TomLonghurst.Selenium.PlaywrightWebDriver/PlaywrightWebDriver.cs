using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Playwright;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

[assembly: InternalsVisibleTo("TomLonghurst.Selenium.PlaywrightWebDriver.Tests")]
namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightWebDriver : IWebDriver, IJavaScriptExecutor, IAsyncDisposable
{
    public readonly IPlaywright Playwright;
    public readonly IBrowser Browser;
    public readonly IBrowserContext Context;

    internal string? _lastAlertText;

    public IPage OriginalPage;

    public IPage CurrentPage
    {
        get => _currentPage;
        internal set
        {
            _currentPage = value;
            CurrentFrameLocators.Clear();
            var handle = PageToKey[value];
            _windowHandlesInOrderSwitchedTo.Add(handle);
            value.BringToFrontAsync().Synchronously();
        }
    }

    public IFrameLocator? CurrentFrame => CurrentFrameLocators.LastOrDefault();
        
    internal readonly HashSet<IFrameLocator> CurrentFrameLocators = new();

    private readonly List<string> _windowHandlesInOrderSwitchedTo = new();
    internal readonly Dictionary<string, IPage> KeyToPage = new();
    private IPage _currentPage;
    private Dictionary<IPage, string> PageToKey => KeyToPage.ToDictionary(x => x.Value, x => x.Key);

    public PlaywrightWebDriver(IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage currentPage)
    {
        Playwright = playwright;
        Browser = browser;
        Context = context;
        OriginalPage = currentPage;
            
        var key = Guid.NewGuid().ToString();
        KeyToPage.Add(key, currentPage);
            
        CurrentPage = currentPage;
            
        RegisterAlertHandler(currentPage);
    }

    private void RegisterAlertHandler(IPage currentPage)
    {
        currentPage.Dialog += (sender, dialog) =>
        {
            _lastAlertText = dialog.Message;
            dialog.DismissAsync();
        };
    }

    public static PlaywrightWebDriver Create()
    {
        return CreateAsync().Synchronously();
    }
    
    public static Task<PlaywrightWebDriver> CreateAsync()
    {
        return CreateAsync(PlaywrightBrowserType.Chromium, new BrowserTypeLaunchOptions(), new BrowserNewContextOptions());
    }

    public static PlaywrightWebDriver Create(BrowserTypeLaunchOptions browserTypeLaunchOptions,
        BrowserNewContextOptions browserNewContextOptions)
    {
        return CreateAsync(PlaywrightBrowserType.Chromium, browserTypeLaunchOptions, browserNewContextOptions).Synchronously();
    }
    
    public static async Task<PlaywrightWebDriver> CreateAsync(PlaywrightBrowserType playwrightBrowserType, BrowserTypeLaunchOptions browserTypeLaunchOptions, BrowserNewContextOptions browserNewContextOptions)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        var browserType = GetBrowserType(playwright, playwrightBrowserType);
        
        var browser = await browserType.LaunchAsync(browserTypeLaunchOptions);

        var context = await browser.NewContextAsync(browserNewContextOptions);

        var page = await context.NewPageAsync();

        return new PlaywrightWebDriver(playwright, browser, context, page);
    }

    private static IBrowserType GetBrowserType(IPlaywright playwright, PlaywrightBrowserType playwrightBrowserType)
    {
        return playwrightBrowserType switch
        {
            PlaywrightBrowserType.Chromium => playwright.Chromium,
            PlaywrightBrowserType.Firefox => playwright.Firefox,
            PlaywrightBrowserType.WebKit => playwright.Webkit,
            _ => throw new ArgumentOutOfRangeException(nameof(playwrightBrowserType), playwrightBrowserType, null)
        };
    }


    public void Dispose()
    {
        CastAndDispose(Context);
        CastAndDispose(Browser);
        Playwright.Dispose();

        return;

        static void CastAndDispose(IAsyncDisposable resource)
        {
            if (resource is IDisposable resourceDisposable)
            {
                resourceDisposable.Dispose();
            }
            else
            {
                _ = resource.DisposeAsync().AsTask();
            }
        }
    }

    public IWebElement FindElement(By by)
    {
        var locatorString = LocatorHelpers.GetLocatorString(by);

        var locator = CurrentFrameLocators.Any()
            ? CurrentFrameLocators.Last().Locator(locatorString).First
            : CurrentPage.Locator(locatorString).First;

        if (locator.CountAsync().Synchronously() == 0)
        {
            throw new NoSuchElementException($"No element found for {by.GetLocatorString()}");
        }
            
        return new PlaywrightWebElement(locator, locatorString);
    }

    public ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        var locatorString = LocatorHelpers.GetLocatorString(by);

        var locator = CurrentFrameLocators.Any()
            ? CurrentFrameLocators.Last().Locator(locatorString)
            : CurrentPage.Locator(locatorString);

        var playwrightWebElements = locator
            .AllAsync()
            .Synchronously()
            .Select(x => new PlaywrightWebElement(x, locatorString))
            .Cast<IWebElement>()
            .ToList();
            
        return new ReadOnlyCollection<IWebElement>(playwrightWebElements);
    }

    public void Close()
    {
        var handle = PageToKey[CurrentPage];
        CurrentPage.CloseAsync().Synchronously();
        KeyToPage.Remove(handle);
        _windowHandlesInOrderSwitchedTo.RemoveAll(x => x == handle);

        if (!PageToKey.Any())
        {
            Dispose();
        }
        else
        {
            CurrentPage = KeyToPage[_windowHandlesInOrderSwitchedTo.Last()];
        }
    }

    public void Quit()
    {
        Dispose();
    }

    public IOptions Manage()
    {
        return new PlaywrightOptions(this);
    }

    public INavigation Navigate()
    {
        return new PlaywrightNavigation(this);
    }

    public ITargetLocator SwitchTo()
    {
        return new PlaywrightTargetLocator(this);
    }

    public string Url
    {
        get => CurrentPage.Url;
        set => CurrentPage.GotoAsync(value).Synchronously();
    }

    public string Title => CurrentPage.TitleAsync().Synchronously();

    public string PageSource => CurrentPage.ContentAsync().Synchronously();

    public string CurrentWindowHandle => PageToKey[CurrentPage];

    public ReadOnlyCollection<string> WindowHandles => new(KeyToPage.Keys.Select(x => x.ToString()).ToList());

    public object? ExecuteScript(string script, params object[] args)
    {
        if (CurrentFrameLocators.Any())
        {
            return CurrentFrameLocators.Last().Owner.EvaluateAsync(script, args).Synchronously();
        }
            
        return CurrentPage.EvaluateAsync(script, args).Synchronously();
    }

#if SeleniumVersion_4
    public object? ExecuteScript(PinnedScript script, params object[] args)
    {
        return ExecuteScript(script.Source, args);
    }
#endif

    public object? ExecuteAsyncScript(string script, params object[] args)
    {
        if (CurrentFrameLocators.Any())
        {
            return CurrentFrameLocators.Last().Owner.EvaluateAsync(script, args).Synchronously();
        }
            
        return CurrentPage.EvaluateAsync(script, args).Synchronously();
    }
        
    internal PlaywrightWebDriver NewPage()
    {
        var page = Context.NewPageAsync().Synchronously();
            
        var key = Guid.NewGuid().ToString();
        KeyToPage.Add(key, page);
            
        CurrentPage = page;
            
        RegisterAlertHandler(page);
            
        return this;
    }

    public PlaywrightWebDriver SetFrame(IFrameLocator frameLocator)
    {
        CurrentFrameLocators.Add(frameLocator);
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}