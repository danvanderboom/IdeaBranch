using System.Runtime.InteropServices;

namespace IdeaBranch.UITests.Infrastructure;

/// <summary>
/// Configuration for Appium test execution.
/// Supports Windows and iOS platforms.
/// </summary>
public sealed class AppiumConfiguration
{
    /// <summary>
    /// Gets the current platform (Windows or iOS).
    /// </summary>
    public static Platform Platform => GetPlatform();

    /// <summary>
    /// Gets the Appium server URL for the current platform.
    /// </summary>
    public string AppiumServerUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the app executable path (Windows) or bundle ID (iOS).
    /// </summary>
    public string AppPathOrBundleId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets platform-specific capabilities.
    /// </summary>
    public Dictionary<string, object> Capabilities { get; } = new();

    private AppiumConfiguration()
    {
    }

    /// <summary>
    /// Creates Windows-specific Appium configuration.
    /// </summary>
    public static AppiumConfiguration CreateWindowsConfig(string appExecutablePath, string winAppDriverUrl = "http://127.0.0.1:4723")
    {
        if (string.IsNullOrWhiteSpace(appExecutablePath))
            throw new ArgumentException("App executable path is required", nameof(appExecutablePath));

        var config = new AppiumConfiguration
        {
            AppiumServerUrl = winAppDriverUrl,
            AppPathOrBundleId = appExecutablePath
        };

        // Windows-specific capabilities for WinAppDriver
        config.Capabilities["platformName"] = "Windows";
        config.Capabilities["deviceName"] = "WindowsPC";
        config.Capabilities["app"] = appExecutablePath;
        config.Capabilities["automationName"] = "Windows"; // UIA3
        config.Capabilities["ms:experimental-webdriver"] = true;
        config.Capabilities["ms:waitForAppLaunch"] = "25";

        return config;
    }

    /// <summary>
    /// Creates iOS-specific Appium configuration.
    /// </summary>
    public static AppiumConfiguration CreateiOSConfig(string bundleId, string appiumServerUrl = "http://127.0.0.1:4723", string? deviceName = null, string? platformVersion = null)
    {
        if (string.IsNullOrWhiteSpace(bundleId))
            throw new ArgumentException("Bundle ID is required", nameof(bundleId));

        var config = new AppiumConfiguration
        {
            AppiumServerUrl = appiumServerUrl,
            AppPathOrBundleId = bundleId
        };

        // iOS-specific capabilities for XCUITest
        config.Capabilities["platformName"] = "iOS";
        config.Capabilities["deviceName"] = deviceName ?? "iPhone 15";
        config.Capabilities["platformVersion"] = platformVersion ?? "17.0";
        config.Capabilities["bundleId"] = bundleId;
        config.Capabilities["automationName"] = "XCUITest";
        config.Capabilities["noReset"] = false;
        config.Capabilities["fullReset"] = false;

        return config;
    }

    /// <summary>
    /// Creates configuration from environment variables.
    /// </summary>
    public static AppiumConfiguration CreateFromEnvironment()
    {
        var platform = GetPlatform();
        var appPath = Environment.GetEnvironmentVariable("MAUI_APP_PATH");
        var bundleId = Environment.GetEnvironmentVariable("MAUI_BUNDLE_ID");
        var appiumUrl = Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://127.0.0.1:4723";

        return platform switch
        {
            Platform.Windows => CreateWindowsConfig(
                appPath ?? GetDefaultWindowsAppPath(),
                appiumUrl),
            Platform.iOS => CreateiOSConfig(
                bundleId ?? GetDefaultiOSBundleId(),
                appiumUrl),
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };
    }

    private static Platform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Platform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Platform.iOS;
        
        throw new NotSupportedException("Only Windows and iOS are currently supported");
    }

    private static string GetDefaultWindowsAppPath()
    {
        // Default path for Windows app executable
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var appPath = Path.Combine(basePath, "src", "IdeaBranch.App", "bin", "Debug", "net9.0-windows10.0.19041.0", "win10-x64", "IdeaBranch.App.exe");
        
        return File.Exists(appPath) ? appPath : throw new FileNotFoundException($"Default app path not found: {appPath}");
    }

    private static string GetDefaultiOSBundleId()
    {
        return "com.companyname.ideabranch.app";
    }
}

/// <summary>
/// Supported platforms for UI automation.
/// </summary>
public enum Platform
{
    Windows,
    iOS
}

