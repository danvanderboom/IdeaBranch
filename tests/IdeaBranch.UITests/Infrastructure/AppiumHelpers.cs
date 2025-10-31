using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Appium;
using System.Runtime.InteropServices;

namespace IdeaBranch.UITests.Infrastructure;

/// <summary>
/// Helper methods for Appium UI automation operations.
/// </summary>
public static class AppiumHelpers
{
    /// <summary>
    /// Gets the locator for finding elements by AutomationId based on the platform.
    /// </summary>
    private static By GetAutomationIdLocator(string automationId)
    {
        // For Windows WinAppDriver (UIA3), AutomationId maps to Name property
        // For iOS, we would use AccessibilityId
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return By.Name(automationId);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // iOS uses AccessibilityId for AutomationId
            return MobileBy.AccessibilityId(automationId);
        }
        
        // Default fallback
        return By.Name(automationId);
    }

    /// <summary>
    /// Finds an element by AutomationId with explicit wait.
    /// </summary>
    public static IWebElement FindElementByAutomationId(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        var locator = GetAutomationIdLocator(automationId);
        return wait.Until(ExpectedConditions.ElementExists(locator));
    }

    /// <summary>
    /// Finds an element by AutomationId, returning null if not found.
    /// </summary>
    public static IWebElement? TryFindElementByAutomationId(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        try
        {
            return driver.FindElementByAutomationId(automationId, timeout);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// Clicks an element by AutomationId.
    /// </summary>
    public static void ClickElementByAutomationId(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var element = driver.FindElementByAutomationId(automationId, timeout);
        element.Click();
    }

    /// <summary>
    /// Waits for an element to be clickable and then clicks it.
    /// </summary>
    public static void WaitAndClickElement(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        var locator = GetAutomationIdLocator(automationId);
        var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        element.Click();
    }

    /// <summary>
    /// Gets text from an element by AutomationId.
    /// </summary>
    public static string GetElementText(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var element = driver.FindElementByAutomationId(automationId, timeout);
        return element.Text;
    }

    /// <summary>
    /// Checks if an element is visible.
    /// </summary>
    public static bool IsElementVisible(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        try
        {
            var element = driver.FindElementByAutomationId(automationId, timeout);
            return element.Displayed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if an element is enabled.
    /// </summary>
    public static bool IsElementEnabled(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        try
        {
            var element = driver.FindElementByAutomationId(automationId, timeout);
            return element.Enabled;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for element text to contain specified text.
    /// </summary>
    public static void WaitForElementText(this IWebDriver driver, string automationId, string expectedText, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        wait.Until(driver =>
        {
            try
            {
                var element = driver.FindElementByAutomationId(automationId, TimeSpan.FromSeconds(1));
                return element.Text.Contains(expectedText, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Waits for element to become visible.
    /// </summary>
    public static void WaitForElementVisible(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        var locator = GetAutomationIdLocator(automationId);
        wait.Until(ExpectedConditions.ElementIsVisible(locator));
    }

    /// <summary>
    /// Waits for element to become invisible.
    /// </summary>
    public static void WaitForElementInvisible(this IWebDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        var locator = GetAutomationIdLocator(automationId);
        wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
    }

    /// <summary>
    /// Takes a screenshot and saves it to the specified path.
    /// </summary>
    public static void TakeScreenshot(this IWebDriver driver, string filePath)
    {
        if (driver is ITakesScreenshot takesScreenshot)
        {
            var screenshot = takesScreenshot.GetScreenshot();
            screenshot.SaveAsFile(filePath);
        }
        else
        {
            throw new NotSupportedException("Driver does not support screenshots");
        }
    }

    /// <summary>
    /// Scrolls to an element to make it visible.
    /// </summary>
    public static void ScrollToElement(this IWebDriver driver, string automationId)
    {
        var element = driver.FindElementByAutomationId(automationId);
        
        // For Windows/UIA3, scrolling might be handled differently
        // This is a basic implementation that may need platform-specific adjustments
        try
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        }
        catch
        {
            // JavaScript execution not available on all platforms
            // Element should still be findable without scrolling
        }
    }

    /// <summary>
    /// Finds multiple elements by a common pattern.
    /// </summary>
    public static IReadOnlyList<IWebElement> FindElementsByAutomationIdPattern(this IWebDriver driver, string pattern, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
        var locator = GetAutomationIdLocator(pattern);
        wait.Until(driver => driver.FindElements(locator).Count > 0);
        
        return driver.FindElements(locator).ToList();
    }
}

