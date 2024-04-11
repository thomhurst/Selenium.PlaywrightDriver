using System;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

internal static class LocatorHelpers
{
    public static string GetLocatorString(By by)
    {
        if (by is PlaywrightBy playwrightBy)
        {
            return playwrightBy.PlaywrightLocator;
        }
        
        if (by.Mechanism == "xpath")
        {
            return $"xpath={by.Criteria}";
        }

        if (by.Mechanism == "css selector")
        {
            return $"css={by.Criteria}";
        }

        if (by.Mechanism == "tag name")
        {
            return by.Criteria;
        }

        if (by.Mechanism == "link text")
        {
            return $"a:text-is(\"{by.Criteria}\")";
        }

        if (by.Mechanism == "partial link text")
        {
            return $"a:has-text(\"{by.Criteria}\")";
        }

        throw new ArgumentException($"Unknown Selenium Locator: {by.Mechanism} - {by.Criteria}");
    }
}