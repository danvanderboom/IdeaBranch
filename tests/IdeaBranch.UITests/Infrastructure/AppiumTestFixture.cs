using AppiumOptions = OpenQA.Selenium.Appium.AppiumOptions;
using AppiumDriver = OpenQA.Selenium.Appium.AppiumDriver;
using System.Diagnostics;
using NUnit.Framework;

namespace IdeaBranch.UITests.Infrastructure;

/// <summary>
/// Base test fixture for Appium-based UI tests.
/// Handles app launch, driver initialization, and cleanup.
/// </summary>
public abstract class AppiumTestFixture : IDisposable
{
    protected AppiumDriver? Driver { get; private set; }
    protected AppiumConfiguration Configuration { get; private set; } = null!;
    private Process? _appiumServerProcess;
    private Process? _winAppDriverProcess;

    /// <summary>
    /// Initializes the test fixture with Appium driver.
    /// </summary>
    protected virtual void SetUp()
    {
        // Gate UI tests behind an environment variable to avoid running without proper setup
        var enableUiTests = Environment.GetEnvironmentVariable("ENABLE_UI_TESTS");
        if (!string.Equals(enableUiTests, "1", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(enableUiTests, "true", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("UI tests are disabled. Set ENABLE_UI_TESTS=1 to run them.");
        }

        Configuration = AppiumConfiguration.CreateFromEnvironment();
        StartAppiumServer();
        InitializeDriver();
        LaunchApp();
    }

    /// <summary>
    /// Cleans up the test fixture.
    /// </summary>
    protected virtual void TearDown()
    {
        Driver?.Quit();
        Driver?.Dispose();
        StopAppiumServer();
    }

    private void StartAppiumServer()
    {
        if (AppiumConfiguration.Platform == Platform.Windows)
        {
            StartWinAppDriver();
        }
        else if (AppiumConfiguration.Platform == Platform.iOS)
        {
            // For iOS, Appium server should be started separately
            // This assumes it's already running or started externally
            VerifyAppiumServerRunning();
        }
    }

    private void StartWinAppDriver()
    {
        // Try to start WinAppDriver if not already running
        try
        {
            var winAppDriverPath = Environment.GetEnvironmentVariable("WINAPPDRIVER_PATH") 
                ?? @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

            if (File.Exists(winAppDriverPath))
            {
                _winAppDriverProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = winAppDriverPath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                // Wait for WinAppDriver to start
                Thread.Sleep(2000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not start WinAppDriver: {ex.Message}");
            Console.WriteLine("Assuming WinAppDriver is already running or will be started manually");
        }
    }

    private void VerifyAppiumServerRunning()
    {
        try
        {
            using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = client.GetAsync($"{Configuration.AppiumServerUrl}/status").Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Appium server at {Configuration.AppiumServerUrl} is not responding");
            }
        }
        catch
        {
            throw new InvalidOperationException(
                $"Appium server at {Configuration.AppiumServerUrl} is not running. " +
                "Please start Appium server manually or configure APPIUM_SERVER_URL environment variable.");
        }
    }

    private void InitializeDriver()
    {
        var options = new AppiumOptions();

        // For WinAppDriver, we need to use the exact capability keys it expects
        // Use AddAdditionalAppiumOption with keys that will serialize correctly
        foreach (var capability in Configuration.Capabilities)
        {
            var key = capability.Key;
            var baseKey = key.Contains(":", StringComparison.Ordinal)
                ? key.Substring(key.LastIndexOf(":", StringComparison.Ordinal) + 1)
                : key;
            
            try
            {
                if (AppiumConfiguration.Platform == Platform.Windows)
                {
                    // For WinAppDriver, use lowercase base keys without "appium:" prefix for standard capabilities
                    // WinAppDriver expects "app", "platformName", "deviceName", etc. (not "appium:app")
                    if (!key.Contains(":", StringComparison.Ordinal))
                    {
                        var normalizedBase = baseKey.Trim().ToLowerInvariant();
                        // Use the normalized base key (e.g., "app") - AppiumOptions will add the "appium:" prefix internally
                        options.AddAdditionalAppiumOption(normalizedBase, capability.Value);
                    }
                    else
                    {
                        // For vendor-prefixed capabilities (like "ms:experimental-webdriver"), use original key
                        options.AddAdditionalAppiumOption(capability.Key, capability.Value);
                    }
                }
                else
                {
                    // For iOS, use original key as-is
                    options.AddAdditionalAppiumOption(capability.Key, capability.Value);
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("already an option") || ex.Message.Contains("Application property"))
            {
                // Capability already exists (e.g., if App property was set), skip it
                // This is fine - the capability is already set correctly
                if (baseKey.Equals("app", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Note: 'app' capability already set, skipping duplicate");
                }
            }
        }

        Driver = AppiumConfiguration.Platform switch
        {
            Platform.Windows => new OpenQA.Selenium.Appium.Windows.WindowsDriver(
                new Uri(Configuration.AppiumServerUrl),
                options),
            Platform.iOS => new OpenQA.Selenium.Appium.iOS.IOSDriver(
                new Uri(Configuration.AppiumServerUrl),
                options),
            _ => throw new NotSupportedException($"Platform {AppiumConfiguration.Platform} is not supported")
        };

        // Set implicit wait timeout
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    private void LaunchApp()
    {
        // For Windows, the app is launched automatically via the "app" capability
        // For iOS, the bundle ID is specified and the app should launch
        
        // Wait for app to launch (additional wait for Windows)
        if (AppiumConfiguration.Platform == Platform.Windows)
        {
            Thread.Sleep(3000); // Give app time to launch
        }
    }

    private void StopAppiumServer()
    {
        try
        {
            _winAppDriverProcess?.Kill();
            _winAppDriverProcess?.Dispose();
        }
        catch
        {
            // Ignore errors during cleanup
        }

        try
        {
            _appiumServerProcess?.Kill();
            _appiumServerProcess?.Dispose();
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    public void Dispose()
    {
        TearDown();
    }
}

