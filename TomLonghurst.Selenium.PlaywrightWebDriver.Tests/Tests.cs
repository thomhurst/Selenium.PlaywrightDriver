using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using OpenQA.Selenium;
using Cookie = OpenQA.Selenium.Cookie;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Tests;

[Parallelizable(ParallelScope.All)]
public class Tests
{
    [Test]
    public async Task Last_Window_Closed()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "TagName.html");

        driver.Close();
        
        Assert.That(driver.KeyToPage, Is.Empty);
    }

    [Test]
    public async Task Script_With_Return()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "LocatorsTest.html");

        var result = driver.ExecuteScript("return JSON.stringify(document.getElementById('id1').textContent)")!.ToString();
        
        Assert.That(result, Is.EqualTo("\"Id One\""));
    }
    
    [Test]
    public async Task Script_Without_Return()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "LocatorsTest.html");

        var result = driver.ExecuteScript("JSON.stringify(document.getElementById('id1').textContent)")!.ToString();
        
        Assert.That(result, Is.EqualTo("\"Id One\""));
    }
    
    [Test]
    public async Task Script_With_Selenium_Style_Arguments()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "LocatorsTest.html");

        var result = driver.ExecuteScript("`You passed in ${arguments[0]} and ${arguments[1]}`", "Foo", "Bar")!.ToString();
        
        Assert.That(result, Is.EqualTo("You passed in Foo and Bar"));
    }
    
    [Test]
    public async Task Script_With_Element_Argument()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "ScriptElementScroll.html");

        var idElement = driver.FindElement(By.Id("id500"));

        driver.ExecuteScript("arguments[0].scrollIntoView(true);", idElement);
        
        Console.WriteLine();
    }

    [Test]
    public async Task Locators_Test()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "LocatorsTest.html");

        var idLocatorText = driver.FindElement(By.Id("id1")).Text;
        var classLocatorText = driver.FindElement(By.ClassName("class1")).Text;
        var tagLocatorText = driver.FindElement(By.TagName("span")).Text;
        var cssLocatorText = driver.FindElement(By.CssSelector("#id1")).Text;
        var xpathLocatorText = driver.FindElement(By.XPath("//*[@id='id1']")).Text;
        var linkTextLocatorText = driver.FindElement(By.LinkText("Link Text")).Text;
        var partialLinkTextLocatorText = driver.FindElement(By.PartialLinkText("Link")).Text;
        var nameLocatorText = driver.FindElement(By.Name("name1")).Text;
        
        Assert.Multiple(() =>
        {
            Assert.That(idLocatorText, Is.EqualTo("Id One"));
            Assert.That(classLocatorText, Is.EqualTo("Class One"));
            Assert.That(tagLocatorText, Is.EqualTo("Span One"));
            Assert.That(cssLocatorText, Is.EqualTo("Id One"));
            Assert.That(xpathLocatorText, Is.EqualTo("Id One"));
            Assert.That(linkTextLocatorText, Is.EqualTo("Link Text"));
            Assert.That(partialLinkTextLocatorText, Is.EqualTo("Link Text"));
            Assert.That(nameLocatorText, Is.EqualTo("Name One"));
        });
    }

    [Test]
    public async Task TagName()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "TagName.html");

        var host = driver.FindElement(By.Id("host"));
        
        Assert.That(host.TagName, Is.EqualTo("DIV"));
    }

#if !SeleniumVersion_3
    [Test]
    public async Task ShadowDom()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "ShadowDom.html");

        var span = driver.FindElement(By.Id("host")).GetShadowRoot().FindElement(By.TagName("span"));
        
        Assert.That(span.Text, Is.EqualTo("I'm in the shadow DOM"));
    }
#endif
    
    [Test]
    public async Task NestedFinds()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "NestedFinds.html");

        var span = driver.FindElement(By.Id("host"))
            .FindElement(By.ClassName("inner1"))
            .FindElement(By.ClassName("inner2"))
            .FindElement(By.ClassName("inner3"));
        
        Assert.That(span.Text.Trim(), Is.EqualTo("Most inner!"));
    }
    
    [Test]
    public async Task Click()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Click.html");

        var originalText = driver.FindElement(By.Id("host")).Text;
            
        Assert.That(originalText, Is.EqualTo("Not yet clicked"));
        
        driver.FindElement(By.Id("btn")).Click();
        
        var newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Clicked"));
    }
    
    [Test]
    public async Task Alert()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Alert.html");
        
        Assert.That(driver.FindElement(By.Id("host")).Text, Is.EqualTo("Foo bar"));
        
        Assert.That(driver.SwitchTo().Alert().Text, Is.EqualTo("Hello! I am an alert box!!"));

        Assert.That(driver.FindElement(By.Id("host")).Text, Is.EqualTo("Foo bar"));
    }
    
    [Test]
    public async Task Cookie()
    {
        await using var webApp = WebApplication.Create();

        webApp.MapGet("/cookie", context =>
        {
            context.Response.Cookies.Append("PlaywrightSeleniumInterop", "Blah");

            return Task.FromResult(new OkResult());
        });
        
        _ = webApp.StartAsync();

        var address = webApp.Services.GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!
            .Addresses
            .First();

        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = address + "/cookie";
        
        var cookie = driver.Manage().Cookies.GetCookieNamed("PlaywrightSeleniumInterop");
        
        Assert.That(cookie.Value, Is.EqualTo("Blah"));
        
        driver.Manage().Cookies.DeleteCookieNamed("PlaywrightSeleniumInterop");
        
        cookie = driver.Manage().Cookies.GetCookieNamed("PlaywrightSeleniumInterop");
        
        Assert.That(cookie, Is.Null);
        
        driver.Manage().Cookies.AddCookie(new Cookie("MyCookie", "Hello world!"));
        
        cookie = driver.Manage().Cookies.GetCookieNamed("MyCookie");
        
        Assert.That(cookie.Value, Is.EqualTo("Hello world!"));
    }
    
    [Test]
    public async Task FormSubmit()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "FormSubmit.html");
        
        var originalText = driver.FindElement(By.Id("host")).Text;
        
        Assert.That(originalText, Is.EqualTo("Blah"));

        var input = driver.FindElement(By.Id("input1"));
        input.SendKeys("Hello, world!");
        
        driver.FindElement(By.Id("form1")).Submit();
        
        var newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Hello, world!"));
    }

    [Test]
    public async Task IFrame()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "IFrame.html");
        
        Assert.That(driver.FindElement(By.Id("iframe-root-page1")).Text, Is.EqualTo("Root Page"));
        Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("host")));

        // Element is not a frame
        Assert.Throws<NoSuchFrameException>(() => driver.SwitchTo().Frame(driver.FindElement(By.Id("iframe-root-page1"))));
        
        driver.SwitchTo().Frame(driver.FindElement(By.Id("frame1")));
        
        Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("iframe-root-page1")));
        Assert.That(driver.FindElement(By.Id("host")).Text, Is.EqualTo("Window1!"));

        driver.SwitchTo().ParentFrame();
        
        Assert.That(driver.FindElement(By.Id("iframe-root-page1")).Text, Is.EqualTo("Root Page"));
        Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("host")));
    }
    
    [Test]
    public async Task Popup()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Popup.html");
        
        Assert.Throws<NoSuchElementException>(() => driver.FindElement(By.Id("host")));

        var otherWindowHandle = driver.WindowHandles.First(x => x != driver.CurrentWindowHandle);
        driver.SwitchTo().Window(otherWindowHandle);
        
        Assert.That(driver.FindElement(By.Id("host")).Text, Is.EqualTo("Window1!")); 
    }

#if !SeleniumVersion_3
    [Test]
    public async Task SwitchTabs()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Windows", "Window1.html");
        
        var originalText = driver.FindElement(By.Id("host")).Text;
        
        Assert.That(originalText, Is.EqualTo("Window1!"));

        driver.SwitchTo().NewWindow(WindowType.Window);
        
        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Windows", "Window2.html");
        
        var newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window2!"));
        
        Assert.Throws<NoSuchWindowException>(() => driver.SwitchTo().Window("non-existing-window-handle"));
        
        driver.SwitchTo().Window(driver.WindowHandles.First());
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window1!"));
        
        driver.SwitchTo().Window(driver.WindowHandles.Last());
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window2!"));
        
        driver.Close();
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window1!"));
    }
#endif
    
#if !SeleniumVersion_3
    [Test]
    public async Task SwitchToDefaultContent()
    {
        await using var driver = await PlaywrightWebDriver.CreateAsync();

        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Windows", "Window1.html");
        
        var originalText = driver.FindElement(By.Id("host")).Text;
        
        Assert.That(originalText, Is.EqualTo("Window1!"));

        driver.SwitchTo().NewWindow(WindowType.Window);
        
        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Windows", "Window2.html");
        
        var newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window2!"));

        driver.SwitchTo().NewWindow(WindowType.Window);
        
        driver.Url = "file://" + Path.Combine(Environment.CurrentDirectory, "HtmlPages", "Windows", "Window3.html");
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window3!"));
        
        driver.SwitchTo().DefaultContent();
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window1!"));
        
        driver.SwitchTo().Window(driver.WindowHandles.Last());

        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window3!"));
        
        driver.SwitchTo().Window(driver.WindowHandles.First());

        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window1!"));
        
        driver.SwitchTo().Window(driver.WindowHandles[1]);

        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window2!"));
        
        driver.Close();
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window1!"));
        
        driver.Close();
        
        newText = driver.FindElement(By.Id("host"));
        
        Assert.That(newText.Text, Is.EqualTo("Window3!"));
        
        driver.Close();
    }
#endif
}