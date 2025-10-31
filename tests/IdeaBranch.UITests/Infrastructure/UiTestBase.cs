using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace IdeaBranch.UITests.Infrastructure;

/// <summary>
/// Base class for UI tests that provides artifact capture on failure and storage cleanup.
/// Extends AppiumTestFixture to add artifact capture and cleanup patterns.
/// </summary>
public abstract class UiTestBase : AppiumTestFixture
{
    private string? _artifactsDirectory;

    /// <summary>
    /// Gets the artifacts directory for this test run.
    /// </summary>
    protected string ArtifactsDirectory
    {
        get
        {
            if (_artifactsDirectory == null)
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var testName = TestContext.CurrentContext.Test.Name;
                _artifactsDirectory = Path.Combine("artifacts", "tests", "ui", timestamp, testName);
                Directory.CreateDirectory(_artifactsDirectory);
            }
            return _artifactsDirectory;
        }
    }

    /// <summary>
    /// Sets up the test. Override to add custom setup.
    /// </summary>
    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
        _artifactsDirectory = null; // Reset for each test
    }

    /// <summary>
    /// Tears down the test with artifact capture and cleanup.
    /// </summary>
    [TearDown]
    protected override void TearDown()
    {
        try
        {
            // Capture artifacts on failure
            var testContext = TestContext.CurrentContext;
            if (testContext.Result.Outcome.Status.ToString() == "Failed" && Driver != null)
            {
                CaptureArtifacts();
            }

            // Clear storage
            ClearStorage();
        }
        catch (Exception ex)
        {
            // Log but don't fail teardown
            TestContext.WriteLine($"Error during teardown: {ex.Message}");
        }
        finally
        {
            base.TearDown();
        }
    }

    /// <summary>
    /// Captures screenshots and logs on test failure.
    /// </summary>
    protected virtual void CaptureArtifacts()
    {
        if (Driver == null)
            return;

        try
        {
            // Capture screenshot
            var screenshotPath = Path.Combine(ArtifactsDirectory, "screenshot.png");
            if (Driver is ITakesScreenshot takesScreenshot)
            {
                var screenshot = takesScreenshot.GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);
                TestContext.WriteLine($"Screenshot saved to: {screenshotPath}");
            }

            // Capture logs
            var logPath = Path.Combine(ArtifactsDirectory, "driver.log");
            CaptureDriverLogs(logPath);

            // Capture test context
            var contextPath = Path.Combine(ArtifactsDirectory, "test-context.txt");
            File.WriteAllText(contextPath, $@"Test: {TestContext.CurrentContext.Test.Name}
Status: {TestContext.CurrentContext.Result.Outcome.Status}
Message: {TestContext.CurrentContext.Result.Message}
StackTrace: {TestContext.CurrentContext.Result.StackTrace}
");
            TestContext.WriteLine($"Artifacts saved to: {ArtifactsDirectory}");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to capture artifacts: {ex.Message}");
        }
    }

    /// <summary>
    /// Captures driver logs to a file.
    /// </summary>
    /// <param name="logPath">The path to save the log file.</param>
    protected virtual void CaptureDriverLogs(string logPath)
    {
        if (Driver == null)
            return;

        try
        {
            var logs = new List<string>();
            
            // Try to get browser/driver logs if available
            try
            {
                var logEntries = Driver.Manage().Logs.GetLog(LogType.Browser);
                foreach (var entry in logEntries)
                {
                    logs.Add($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}");
                }
            }
            catch
            {
                // Browser logs may not be available for all driver types
            }

            // Try to get driver logs
            try
            {
                var driverLogs = Driver.Manage().Logs.GetLog(LogType.Driver);
                foreach (var entry in driverLogs)
                {
                    logs.Add($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}");
                }
            }
            catch
            {
                // Driver logs may not be available for all driver types
            }

            // Write logs to file
            if (logs.Count > 0)
            {
                File.WriteAllLines(logPath, logs);
            }
            else
            {
                File.WriteAllText(logPath, "No logs available for this driver type.");
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText(logPath, $"Failed to capture logs: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears local and session storage.
    /// </summary>
    protected virtual void ClearStorage()
    {
        if (Driver == null)
            return;

        try
        {
            // Clear local storage if supported
            try
            {
                Driver.ExecuteScript("localStorage.clear();");
            }
            catch
            {
                // localStorage may not be available for all driver types
            }

            // Clear session storage if supported
            try
            {
                Driver.ExecuteScript("sessionStorage.clear();");
            }
            catch
            {
                // sessionStorage may not be available for all driver types
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Failed to clear storage: {ex.Message}");
        }
    }
}

