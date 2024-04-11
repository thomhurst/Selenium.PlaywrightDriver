using System.Reflection;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Helpers;

internal static class ByExtensions
{
    public static string GetLocatorString(this By by)
    {
#if SeleniumVersion_4
        return $"{by.Mechanism} - {by.Criteria}";
#else
    return by.GetDescription();
#endif
    }

    public static string GetDescription(this By by)
    {
        return by.GetType()
            .GetField("description", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(by)
            .ToString();
    }
}