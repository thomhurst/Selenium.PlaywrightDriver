using System;
using System.Linq;
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

        return GetInternal(by);
    }

#if SeleniumVersion_4
    private static string GetInternal(By by)
    {
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
#endif

#if SeleniumVersion_3
    private static string GetInternal(By by)
    {
        var description = by.GetDescription();
        var criteria = description.Split(':').Last();
        if (description.StartsWith("By.XPath:"))
        {
            return $"xpath={criteria}";
        }

        if (description.StartsWith("By.CssSelector:"))
        {
            return $"css={criteria}";
        }

        if (description.StartsWith("By.TagName:"))
        {
            return criteria;
        }

        if (description.StartsWith("By.LinkText:"))
        {
            return $"a:text-is(\"{criteria}\")";
        }

        if (description.StartsWith("By.PartialLinkText:"))
        {
            return $"a:has-text(\"{criteria}\")";
        }

        throw new ArgumentException($"Unknown Selenium Locator: {description}");
    }
#endif
}