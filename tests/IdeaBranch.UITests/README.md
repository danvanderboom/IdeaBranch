# Idea Branch UI Tests

This project contains UI automation tests for the Idea Branch .NET MAUI application using Appium-based automation framework.

## Overview

The UI tests use Appium for cross-platform UI automation:
- **Windows**: WinAppDriver (UIA3)
- **iOS**: XCUITest (planned)

## Test Structure

### Test Classes

- **`ResilienceTests`**: Tests for `ResilienceTestPage`
  - Navigation to Resilience Test page
  - Button interactions (GET, POST, Retry, Circuit Breaker, Delay)
  - Status message updates
  - Results display verification
  - Button state during busy operations

- **`SmokeTests`**: Basic smoke tests
  - App launch verification
  - Primary navigation AutomationId checks
  - Navigation to Topic Tree, Map, Timeline (awaiting implementation)

- **`TopicTreeTests`**: Tests for TopicTree page (awaiting implementation)
  - Hierarchical node display
  - Expand/collapse functionality
  - Depth-based indentation
  - Stable AutomationIds

- **`ErrorHandlingTests`**: Tests for error handling scenarios
  - Network unavailable (IB-UI-050)
  - Model/API error handling (IB-UI-051)
  - App responsiveness after errors
  - Error message display verification

- **`AccessibilityTests`**: Tests for accessibility requirements
  - Screen reader support - AutomationId presence (IB-UI-040)
  - Keyboard navigation (IB-UI-041)
  - Focus order and keyboard accessibility
  - Element accessibility verification

- **`LocalizationTests`**: Tests for localization (awaiting Settings UI)
  - Language switching and persistence (IB-UI-030)
  - String updates when language changes (IB-UI-031)
  - Locale-aware date formatting (IB-UI-032)
  - Placeholder tests ready for Settings/Localization UI

- **`NotificationTests`**: Tests for notifications (awaiting Notification UI)
  - Due date reminder appearance (IB-UI-070)
  - Push notification settings (IB-UI-071)
  - In-app notification behavior
  - Placeholder tests ready for Notification UI

### Infrastructure

- **`AppiumTestFixture`**: Base class for all UI tests
  - Handles driver initialization
  - App launch and cleanup
  - Platform-specific setup

- **`AppiumHelpers`**: Helper methods for common operations
  - Element finding by AutomationId
  - Click, tap, text input
  - Wait utilities
  - Screenshot support

- **`AppiumConfiguration`**: Platform configuration
  - Windows/iOS capability setup
  - Environment variable handling
  - Default path resolution

## Prerequisites

### Windows
1. .NET 9 SDK
2. WinAppDriver installed and running
3. Developer Mode enabled in Windows Settings
4. Built MAUI app executable

See [Appium Setup Guide](../../docs/testing/appium-setup.md) for detailed setup instructions.

## Running Tests

### Local Execution

1. **Start WinAppDriver**:
   ```powershell
   "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
   ```

2. **Build the app**:
   ```bash
   dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Debug -f net9.0-windows10.0.19041.0
   ```

3. **Run tests**:
   ```bash
   dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
   ```

### Using PowerShell Script

```powershell
.\tests\IdeaBranch.UITests\scripts\run-ui-tests.ps1 -Configuration Debug
```

Options:
- `-Configuration`: Debug or Release (default: Debug)
- `-AppPath`: Custom app executable path
- `-TestFilter`: NUnit test filter
- `-SkipWinAppDriver`: Skip starting WinAppDriver (assumes already running)
- `-Cleanup`: Stop WinAppDriver after tests

### Running Specific Tests

```bash
# Run specific test class
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj --filter "FullyQualifiedName~ResilienceTests"

# Run specific test
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj --filter "TestId=RESILIENCE-001"

# Run by category
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj --filter "TestCategory=Smoke"
```

## Environment Variables

Set these environment variables (optional):

```powershell
# Windows app path
$env:MAUI_APP_PATH = "C:\Path\To\IdeaBranch.App.exe"

# WinAppDriver path
$env:WINAPPDRIVER_PATH = "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"

# Appium server URL (default: http://127.0.0.1:4723)
$env:APPIUM_SERVER_URL = "http://127.0.0.1:4723"
```

## Writing Tests

### Basic Test Structure

```csharp
using IdeaBranch.UITests.Infrastructure;
using NUnit.Framework;

public class MyTests : AppiumTestFixture
{
    [SetUp]
    public void Setup() => SetUp();
    
    [TearDown]
    public void TearDown() => base.TearDown();
    
    [Test]
    public void MyTest()
    {
        // Use Driver! to interact with the app
        Driver!.ClickElementByAutomationId("MyButton");
        Driver!.WaitForElementText("Status", "Complete");
        
        var text = Driver!.GetElementText("ResultLabel");
        Assert.AreEqual("Expected", text);
    }
}
```

### Helper Methods

- `FindElementByAutomationId(automationId)` - Find element by AutomationId
- `ClickElementByAutomationId(automationId)` - Click element
- `WaitForElementVisible(automationId)` - Wait for element to appear
- `GetElementText(automationId)` - Get element text
- `IsElementEnabled(automationId)` - Check if element is enabled
- `TakeScreenshot(filePath)` - Take screenshot

## Troubleshooting

### Common Issues

1. **WinAppDriver not running**: Start WinAppDriver manually or use the PowerShell script
2. **App not found**: Verify `MAUI_APP_PATH` is set correctly or build the app
3. **Element not found**: Verify AutomationId is set in XAML and element is visible
4. **Timeout errors**: Increase timeout values or check if app is responsive

See [Appium Setup Guide](../../docs/testing/appium-setup.md#troubleshooting) for detailed troubleshooting.

## CI/CD

Tests run automatically in CI via GitHub Actions:
- Workflow: `.github/workflows/ui-tests.yml`
- Automatically installs WinAppDriver
- Builds app and runs tests
- Publishes test results

See [CI/CD Integration](../../docs/testing/appium-setup.md#cicd-integration) for details.

## Documentation

- [UI Automation Overview](../../docs/testing/ui-automation.md)
- [Appium Setup Guide](../../docs/testing/appium-setup.md)
- [Testing Plan](../../docs/testing/testing-plan.md)
