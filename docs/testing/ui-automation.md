## UI Automation (Appium-based for .NET MAUI)

This document outlines how we run UI automation for Idea Branch using Appium-based automation framework. Currently supports Windows via WinAppDriver, with iOS support planned.

### Goals
- Smoke validation of launch and primary navigation
- Stable `AutomationId` coverage for critical UI
- Runnable locally and in CI

### Prerequisites
- .NET 9 SDK
- Windows 10/11 with Desktop development tools
- WinAppDriver installed (for Windows UI automation)
- Appium server (for iOS, when implemented)

#### Windows Setup
1. Install WinAppDriver:
   - Download from: https://github.com/microsoft/WinAppDriver/releases
   - Install and ensure WinAppDriver is running (default: http://127.0.0.1:4723)
   - Enable Developer Mode in Windows Settings

2. Build the MAUI app:
   ```bash
   dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Debug -f net9.0-windows10.0.19041.0 -p:TargetFramework=net9.0-windows10.0.19041.0
   ```

3. Set environment variables (optional):
   ```bash
   set MAUI_APP_PATH=C:\Path\To\IdeaBranch.App.exe
   set WINAPPDRIVER_PATH=C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe
   ```

#### iOS Setup (Future)
- Xcode installed
- iOS Simulator or device
- Appium server with XCUITest driver
- Bundle ID configured

### Project layout
- `tests/IdeaBranch.UITests/` — NUnit-based test project using Appium
  - `Infrastructure/` — Appium test infrastructure
    - `AppiumTestFixture.cs` — Base fixture for Appium setup/teardown
    - `AppiumHelpers.cs` — Helper methods for element finding and interactions
    - `AppiumConfiguration.cs` — Configuration for Windows/iOS platforms
  - `ResilienceTests.cs` — Tests for ResilienceTestPage
  - `SmokeTests.cs` — Basic smoke tests for app launch and navigation
  - `TopicTreeTests.cs` — Tests for TopicTree page (when implemented)
  - `ErrorHandlingTests.cs` — Tests for error handling scenarios (network unavailable, API errors)
  - `AccessibilityTests.cs` — Tests for accessibility (screen reader support, keyboard navigation)
  - `LocalizationTests.cs` — Tests for localization (language switching, locale formatting) - awaiting Settings UI
  - `NotificationTests.cs` — Tests for notifications (in-app, push) - awaiting Notification UI

### Running tests (local)

#### Prerequisites
1. Start WinAppDriver (Windows):
   ```bash
   "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
   ```
   Or set `WINAPPDRIVER_PATH` environment variable.

2. Ensure the app is built (see Prerequisites above).

#### Run tests
```bash
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
```

#### Run specific tests
```bash
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug --filter "TestCategory=Resilience"
```

#### With custom app path
```bash
set MAUI_APP_PATH=C:\Path\To\IdeaBranch.App.exe
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
```

### CI integration
1. Restore and build the app (Windows target):
   ```bash
   dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Release -f net9.0-windows10.0.19041.0
   ```
2. Start WinAppDriver service (Windows)
3. Build and run tests:
   ```bash
   dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Release
   ```
4. Publish artifacts (test results, logs, screenshots)

See `docs/testing/appium-setup.md` for detailed setup instructions.

### Conventions
- Every interactive control in critical flows SHALL have a stable `AutomationId`
- Tests SHOULD avoid timing flakiness; prefer deterministic waits or explicit signals

### CriticalInsight.Data Integration

The TopicTree page uses `CriticalInsight.Data.Hierarchical.TreeView` for hierarchical display:

- **TreeView**: Maintains flattened `ProjectedCollection` observable collection bound to `CollectionView`
- **Depth-based indentation**: `Depth` property on `ITreeNode` converted to `Thickness` via `DepthToThicknessConverter`
- **Expand/collapse**: Tap gesture toggles `SetIsExpanded(node, bool)` on TreeView
- **Adapter layer**: `TopicTreeAdapter` and `TopicTreeViewProvider` bridge domain models to `TreeNode<TopicNodePayload>`

See `src/IdeaBranch.App/Adapters/` for implementation details.

### Appium Architecture

The test framework uses Appium for cross-platform UI automation:

1. **AppiumTestFixture**: Base class that handles:
   - Driver initialization (Windows/iOS)
   - App launch via WinAppDriver or Appium server
   - Cleanup and teardown

2. **AppiumHelpers**: Extension methods for common operations:
   - Find elements by AutomationId
   - Click, tap, text input
   - Wait utilities (explicit waits, polling)
   - Screenshot utilities

3. **AppiumConfiguration**: Platform-specific configuration:
   - Windows: WinAppDriver endpoint, app path, UIA3 capabilities
   - iOS: XCUITest capabilities, bundle ID, device selection

### Test Structure

Tests inherit from `AppiumTestFixture` and use helper methods:

```csharp
public class ResilienceTests : AppiumTestFixture
{
    [SetUp]
    public void Setup() => SetUp();
    
    [Test]
    public void TestSomething()
    {
        Driver!.FindElementByAutomationId("MyButton").Click();
        Driver!.WaitForElementText("Status", "Complete");
    }
}
```

### Next steps
- ✅ Appium infrastructure implemented
- ✅ ResilienceTests wired up
- ✅ SmokeTests wired up
- ✅ ErrorHandlingTests wired up
- ✅ AccessibilityTests wired up
- ✅ LocalizationTests created (awaiting Settings UI)
- ✅ NotificationTests created (awaiting Notification UI)
- ⏳ TopicTreeTests (awaiting TopicTreePage implementation)
- ⏳ iOS support (awaiting iOS app build)
- ⏳ Additional end-to-end flows per product doc sections

### Scenario → Test ID mapping (Windows UI)

Refer to `openspec/changes/add-initial-idea-branch-specs/traceability.md` for the full matrix. Key items:

| Scenario | Test ID |
| --- | --- |
| Cold start within budget (Windows) | IB-UI-001 |
| Primary navigation AutomationIds exist | IB-UI-010 |
| Navigate to Topic Tree | IB-UI-011 |
| Navigate to Map | IB-UI-012 |
| Navigate to Timeline | IB-UI-013 |
| List responses create child nodes | IB-UI-020 |
| Change language persists | IB-UI-030 |
| Switch language updates strings | IB-UI-031 |
| Dates formatted per locale | IB-UI-032 |
| Screen reader announces navigation items | IB-UI-040 |
| Navigate primary views via keyboard | IB-UI-041 |
| Network unavailable | IB-UI-050 |
| Model/API error | IB-UI-051 |
| Navigation event emitted | IB-UI-060 |
| Due date reminder appears | IB-UI-070 |
| Disable push prevents notifications | IB-UI-071 |
| Save topic tree changes | IB-UI-080 |
| Edit offline then sync on reconnect | IB-UI-090 |

