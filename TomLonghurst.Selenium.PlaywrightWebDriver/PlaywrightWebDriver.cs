using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

[assembly: InternalsVisibleTo("TomLonghurst.Selenium.PlaywrightWebDriver.Tests")]
namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightWebDriver : IWebDriver, IJavaScriptExecutor, IAsyncDisposable
{
    public IPlaywright Playwright { get; }
    public IBrowser Browser { get; }
    public IBrowserContext Context { get; }
    public IPage OriginalPage { get; }

    internal string? _lastAlertText;

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

        currentPage.Popup += (sender, page) =>
        {
            KeyToPage.Add(Guid.NewGuid().ToString(), page);
        };
            
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
        script = ConvertScript(script, args);

        if (CurrentFrameLocators.Any())
        {
            return ConvertResult(CurrentFrameLocators.Last().Owner.EvaluateAsync(script, args).Synchronously());
        }

        return ConvertResult(CurrentPage.EvaluateAsync(script, args).Synchronously());
    }

#if SeleniumVersion_4
    public object? ExecuteScript(PinnedScript script, params object[] args)
    {
        return ExecuteScript(script.Source, args);
    }
#endif

    public object? ExecuteAsyncScript(string script, params object[] args)
    {
        script = ConvertScript(script, args);

        if (CurrentFrameLocators.Any())
        {
            return ConvertResult(CurrentFrameLocators.Last().Owner.EvaluateAsync(script, args).Synchronously());
        }

        return ConvertResult(CurrentPage.EvaluateAsync(script, args).Synchronously());
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

    private static object? ConvertResult(object? result)
    {
        if (result is JsonElement jsonElement)
        {
            return ConvertJsonElement(jsonElement);
        }

        return result;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                {
                    return longValue;
                }
                return element.GetDouble();
            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }
                return new ReadOnlyCollection<object?>(list);
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    dict[property.Name] = ConvertJsonElement(property.Value);
                }
                return dict;
            default:
                return element.ToString();
        }
    }

    private static string ConvertScript(string script, object[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is PlaywrightWebElement playwrightWebElement)
            {
                args[i] = playwrightWebElement.Locator.ElementHandleAsync().Synchronously();
            }
        }
        
        if (script.StartsWith("return "))
        {
            script = script.Substring("return ".Length);
        }
        
        if (RegexArguments.IsMatch(script))
        {
            script = $"arguments => {script}";
        }

        return script;
    }

    private static readonly Regex RegexArguments = new Regex(@"arguments\[(?<number>[0-9]+)\]");
}