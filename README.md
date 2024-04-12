# Selenium.PlaywrightDriver
Use Playwright as if it was a Selenium WebDriver

## Installation/Usage
If you're using Selenium v4
- Install package `TomLonghurst.Selenium.PlaywrightWebDriver`

If you're using Selenium V3
- Install package `TomLonghurst.Selenium.V3.PlaywrightWebDriver`

- As per the playwright instructions, you need to run the playwright install script to install browser binaries and such:

`pwsh bin/Debug/netX/playwright.ps1 install`

This can be achieved in C# by something similar to:

```csharp
Process.Start(new ProcessStartInfo("pwsh")
{
    Arguments = "playwright.ps1 install",
    WorkingDirectory = Environment.CurrentDirectory
})!.WaitForExit();
```

- Anywhere you are using concrete types (e.g. `ChromeWebDriver`), change them to the `IWebDriver` interface
- Then just create a `PlaywrightWebDriver` and use that!

```csharp
await using var driver = await PlaywrightWebDriver.CreateAsync();

driver.Url = "https://www.tomlonghurst.com";

driver.FindElement(By.ClassName("logo__img")).Click();
```

The static `Create/CreateAsync` methods can take the Playwright `BrowserTypeLaunchOptions` and `BrowserNewContextOptions` if you want to customise things like browser type, proxys, mobile emulation, etc.

Your `PlaywrightWebDriver` will also expose the underlying instances of `IPlaywright`, `IBrowser` and `IBrowserContext` if you need to use/manipulate those objects for any specific reason.

## Motivation
I've come across lots of code bases where there are old, large test suites written in Selenium. These are important for confirming the health of a system, so we can't just delete them. But it'd also be a lot of work to migrate/convert them all.

Selenium supports different WebDrivers through the `IWebDriver` contract interface. So I thought, is it possible for me to map the Playwright APIs to the Selenium APIs? If we adhere to that interface, we could just drop in a Playwright driver, and we're not actually using the Selenium tech stack anymore.

What's wrong with Selenium anyway? If you're not experiencing any problems, then great. But here's some things I've experienced:
- It's not asynchronous - So extra pressure on the thread pool. (I know this package isn't either, due to the `IWebDriver` interface, but if this does help you migrate, then you'll be in an async Playwright world!)
- It's slow
- Spawning new browser instances just randomly crashes sometimes with unknown errors, or port problems, or connectivity problems
- Waiting for elements/pages is flaky
- Just random "UNKNOWN_ERROR" exceptions!?

## Caveats
- The parameters, arguments and objects returned by `ExecuteScript` may be different, and so your code surrounding scripts may need to be tweaked 
- Alerts auto dismiss. So `Alert.Accept()` or `Alert.Dismiss()` don't actually do anything. `Alert.SendKeys(text)` is not supported.
- The `WebDriver.Manage().Network` functionality has not been implemented
- The `WebDriver.Manage().Logs` functionality has not been implemented
- Sync-over-async. Playwright is async, but Selenium is not. So this was necessary to adhere to the `IWebDriver` interface. This means that this may be slower than a pure playwright solution due to more pressure on the thread pool. There's also always the risk of sync-over-async deadlocks. As such, this package is recommended as a migration strategy, and you should migrate to the pure playwright API if/when possible.
