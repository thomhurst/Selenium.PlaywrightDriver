# Selenium.PlaywrightDriver
Use Playwright as if it was a Selenium WebDriver

## Caveats
- Alerts auto dismiss. So `Alert.Accept()` or `Alert.Dismiss()` don't actually do anything. `Alert.SendKeys(text)` is not supported.
- The `WebDriver.Manage().Network` functionality has not been implemented
- The `WebDriver.Manage().Logs` functionality has not been implemented
- Sync-over-async. Playwright is async, but Selenium is not. So this was necessary to adhere to the `IWebDriver` interface. This means that this may be slower than a pure playwright solution due to more pressure on the thread pool. There's also always the risk of sync-over-async deadlocks. As such, this package is recommended as a migration strategy, and you should migrate to the pure playwright API if/when possible.

```csharp
await using var driver = await PlaywrightWebDriver.CreateAsync();

driver.Url = "https://www.tomlonghurst.com";

driver.FindElement(By.ClassName("logo__img")).Click();
```
