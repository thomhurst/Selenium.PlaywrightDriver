# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A C# library that implements Selenium's `IWebDriver` interface using Playwright as the underlying browser automation engine. This enables existing Selenium test suites to use Playwright as a drop-in replacement without rewriting tests.

Two NuGet packages are published:
- `TomLonghurst.Selenium.PlaywrightWebDriver` — Selenium v4
- `TomLonghurst.Selenium.V3.PlaywrightWebDriver` — Selenium v3

The library targets `netstandard2.0`. Dual Selenium version support is achieved via conditional compilation: `SeleniumVersion_3` and `SeleniumVersion_4` (default) constants, controlled by the `SeleniumVersion` MSBuild property.

## Build & Test Commands

```bash
# Build the solution
dotnet build

# Run tests (NUnit, requires Playwright browsers installed)
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run the full CI pipeline (build + test + pack)
dotnet run -c Release --framework net8.0 --project TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline

# Build for Selenium v3
dotnet build -p:SeleniumVersion=3
```

Tests require Playwright browser binaries. The test project's `GlobalSetUp.cs` runs `pwsh playwright.ps1 install` automatically via `[OneTimeSetUp]`.

## Architecture

### Adapter Pattern (Selenium → Playwright)

The core design maps Selenium's synchronous `IWebDriver` contract onto Playwright's async APIs:

| Selenium Interface | Playwright Adapter | Purpose |
|---|---|---|
| `IWebDriver` | `PlaywrightWebDriver` | Main driver, page navigation, element finding |
| `IWebElement` | `PlaywrightWebElement` | Element interactions (click, text, attributes) |
| `ITargetLocator` | `PlaywrightTargetLocator` | Switching frames, windows, alerts |
| `INavigation` | `PlaywrightNavigation` | Back, forward, goto, refresh |
| `IOptions` | `PlaywrightOptions` | Facade returning timeouts, cookies, window, logs |
| `ICookieJar` | `PlaywrightCookieJar` | Cookie CRUD operations |
| `IWindow` | `PlaywrightWindow` | Window size/position |
| `IAlert` | `PlaywrightAlert` | Alert handling (auto-dismiss, text retrieval only) |
| `IJavaScriptExecutor` | `PlaywrightWebDriver` | Script execution with argument conversion |

### Sync-over-Async

Playwright is fully async; Selenium's `IWebDriver` is sync. The bridge uses `TaskExtensions.Synchronously()` (wraps `GetAwaiter().GetResult()`). This is a known architectural constraint.

### Locator Conversion

`LocatorHelpers` converts Selenium `By` locators to Playwright selectors:
- `By.Id("x")` → `#x`
- `By.ClassName("x")` → `.x`
- `By.CssSelector(...)` → `css=...`
- `By.XPath(...)` → `xpath=...`
- `By.LinkText(...)` → `a:text-is(...)`
- `By.PartialLinkText(...)` → `a:has-text(...)`
- `PlaywrightBy.Selector(...)` — pass native Playwright selectors through Selenium APIs

### Page/Window Management

`PlaywrightWebDriver` tracks multiple pages via `KeyToPage` dictionary (GUID string → `IPage`). Frame navigation uses `CurrentFrameLocators` HashSet for nested frame support. The `CurrentPage` setter automatically clears frame context and brings the page to front.

### Script Execution

`ExecuteScript`/`ExecuteAsyncScript` convert Selenium script conventions to Playwright:
- Wraps `arguments[n]` references into Playwright's function parameter style
- Converts `PlaywrightWebElement` args to `ElementHandle` for Playwright evaluation
- Strips `return` statements where Playwright expects expression evaluation

## Known Limitations (by design)

- `Manage().Logs` — stub, not implemented
- `Manage().Network` — stub, not implemented (Selenium v4 only)
- Alerts auto-dismiss; `Accept()`/`Dismiss()` are no-ops; `SendKeys()` unsupported
- `ExecuteScript` parameter/return value behavior may differ from Selenium

## Project Structure

- **`TomLonghurst.Selenium.PlaywrightWebDriver/`** — Main library (netstandard2.0)
- **`TomLonghurst.Selenium.PlaywrightWebDriver.Tests/`** — NUnit integration tests (net6.0), HTML fixtures in `HtmlPages/`
- **`TomLonghurst.Selenium.PlaywrightWebDriver.Pipeline/`** — ModularPipelines-based CI/CD (build, test, pack, publish to NuGet)

## CI/CD

GitHub Actions workflow (`.github/workflows/dotnet.yml`) runs the Pipeline project. Publishing to NuGet requires manual `workflow_dispatch` with `publish-packages: true` on the `main` branch. The `NuGet__ApiKey` secret is only available in the Production environment.
