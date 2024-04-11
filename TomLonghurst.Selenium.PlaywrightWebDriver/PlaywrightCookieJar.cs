using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Playwright;
using OpenQA.Selenium;
using TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;
using Cookie = OpenQA.Selenium.Cookie;

namespace TomLonghurst.Selenium.PlaywrightWebDriver;

public class PlaywrightCookieJar : ICookieJar
{
    private readonly PlaywrightWebDriver _playwrightWebDriver;

    public PlaywrightCookieJar(PlaywrightWebDriver playwrightWebDriver)
    {
        _playwrightWebDriver = playwrightWebDriver;
    }

    public void AddCookie(Cookie cookie)
    {
        _playwrightWebDriver.Context.AddCookiesAsync(new[]
        {
            MapCookie(cookie)
        }).Synchronously();
    }

    private SameSiteAttribute? MapSameSite(string sameSite)
    {
        if (Enum.TryParse<SameSiteAttribute>(sameSite, true, out var attr))
        {
            return attr;
        }

        return null;
    }

    public Cookie? GetCookieNamed(string name)
    {
        return AllCookies
            .FirstOrDefault(x => x.Name == name);
    }

    public void DeleteCookie(Cookie cookie)
    {
        DeleteCookieNamed(cookie.Name);
    }

    public void DeleteCookieNamed(string name)
    {
        var cookies = AllCookies;

        var cookiesToDelete = cookies.Where(x => x.Name == name);
        
        DeleteAllCookies();
        
        foreach (var cookie in cookies.Except(cookiesToDelete))
        {
            AddCookie(cookie);
        }
    }

    public void DeleteAllCookies()
    {
        _playwrightWebDriver.Context.ClearCookiesAsync().Synchronously();
    }

    public ReadOnlyCollection<Cookie> AllCookies
    {
        get
        {
            var cookies = _playwrightWebDriver.CurrentPage.Context.CookiesAsync().Synchronously();

            return new ReadOnlyCollection<Cookie>(cookies.Select(MapCookie).ToList());
        }
    }

    private Microsoft.Playwright.Cookie MapCookie(Cookie cookie)
    {
        return new Microsoft.Playwright.Cookie
        {
            Domain = cookie.Domain ?? new Uri(_playwrightWebDriver.CurrentPage.Url).Host,
            Expires = cookie.Expiry.HasValue ? ((DateTimeOffset)cookie.Expiry).ToUnixTimeSeconds() : null,
            Name = cookie.Name,
            Path = cookie.Path ?? new Uri(_playwrightWebDriver.CurrentPage.Url).AbsolutePath,
            Secure = cookie.Secure,
            SameSite = MapSameSite(cookie.SameSite),
            Value = cookie.Value,
            HttpOnly = cookie.IsHttpOnly
        };
    }
    
    private Cookie MapCookie(BrowserContextCookiesResult cookie)
    {
        return new Cookie
        (
            domain: cookie.Domain,
            expiry: cookie.Expires == 0 ? null : DateTimeOffset.FromUnixTimeSeconds((long)cookie.Expires).DateTime,
            name: cookie.Name,
            path: cookie.Path,
            secure: cookie.Secure,
            sameSite: cookie.SameSite.ToString(),
            value: cookie.Value,
            isHttpOnly: cookie.HttpOnly
        );
    }
}