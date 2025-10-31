# Appium Setup Guide for .NET MAUI UI Automation

This guide provides detailed instructions for setting up and running Appium-based UI automation tests for Idea Branch.

## Table of Contents
- [Windows Setup](#windows-setup)
- [iOS Setup (Future)](#ios-setup-future)
- [Environment Configuration](#environment-configuration)
- [Running Tests Locally](#running-tests-locally)
- [Troubleshooting](#troubleshooting)
- [CI/CD Integration](#cicd-integration)

## Windows Setup

### Prerequisites

1. **Windows 10/11** with Desktop development tools
2. **.NET 9 SDK** installed
3. **Visual Studio 2022** or **Visual Studio Code** with C# extension (recommended)

### Install WinAppDriver

WinAppDriver is Microsoft's Windows Application Driver for automation.

1. **Download WinAppDriver**:
   - Visit: https://github.com/microsoft/WinAppDriver/releases
   - Download the latest `WindowsApplicationDriver-<version>.msi`

2. **Install WinAppDriver**:
   - Run the installer
   - Default installation path: `C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe`

3. **Enable Developer Mode** (Required):
   - Open Windows Settings
   - Go to **Privacy & Security** → **For developers**
   - Enable **Developer Mode**
   - Restart if prompted

4. **Start WinAppDriver**:
   ```powershell
   # Option 1: Run manually
   "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
   
   # Option 2: Run as service (recommended for CI)
   # WinAppDriver can be installed as a Windows service
   ```

### Build the MAUI App

Build the Idea Branch app for Windows:

```bash
dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Debug -f net9.0-windows10.0.19041.0
```

The executable will be located at:
```
src/IdeaBranch.App/bin/Debug/net9.0-windows10.0.19041.0/win10-x64/IdeaBranch.App.exe
```

### Verify Setup

1. **Check WinAppDriver**:
   ```bash
   # Test WinAppDriver is running
   curl http://127.0.0.1:4723/status
   ```
   Should return: `{"status":0,"value":{"build":{"version":"..."},"os":{"arch":"amd64","name":"windows","version":"..."}}}`

2. **Verify App Build**:
   - Ensure `IdeaBranch.App.exe` exists in the build output directory

## iOS Setup (Future)

### Prerequisites

1. **macOS** with Xcode installed
2. **Xcode Command Line Tools**:
   ```bash
   xcode-select --install
   ```
3. **Appium Server**:
   ```bash
   npm install -g appium
   npm install -g appium-driver-xcuitest
   ```
4. **iOS Simulator** or physical device

### Install Appium Server

```bash
# Install Appium globally
npm install -g appium

# Install XCUITest driver
appium driver install xcuitest

# Verify installation
appium driver list
```

### Configure iOS App

1. **Get Bundle ID**:
   - Open `src/IdeaBranch.App/IdeaBranch.App.csproj`
   - Find `<ApplicationId>` or configure in `Info.plist`
   - Default: `com.companyname.ideabranch.app`

2. **Build for iOS**:
   ```bash
   dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Debug -f net9.0-ios
   ```

### Start Appium Server

```bash
# Start Appium server
appium --port 4723

# Or with specific driver
appium --port 4723 --use-driver xcuitest
```

## Environment Configuration

### Environment Variables

Configure these environment variables (optional):

```bash
# Windows
set MAUI_APP_PATH=C:\Source\CriticalInsight\IdeaBranch\src\IdeaBranch.App\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe
set WINAPPDRIVER_PATH=C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe
set APPIUM_SERVER_URL=http://127.0.0.1:4723

# iOS (future)
set MAUI_BUNDLE_ID=com.companyname.ideabranch.app
set APPIUM_SERVER_URL=http://127.0.0.1:4723
```

### PowerShell Script (Windows)

Create a `test-setup.ps1` script:

```powershell
# Start WinAppDriver
$winAppDriverPath = "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
Start-Process -FilePath $winAppDriverPath -WindowStyle Hidden

# Set environment variables
$env:MAUI_APP_PATH = "C:\Source\CriticalInsight\IdeaBranch\src\IdeaBranch.App\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe"
$env:WINAPPDRIVER_PATH = $winAppDriverPath

# Wait for WinAppDriver to start
Start-Sleep -Seconds 3

# Run tests
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
```

## Running Tests Locally

### Basic Test Execution

```bash
# Run all tests
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug

# Run with specific configuration
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug -v normal

# Run specific test class
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug --filter "FullyQualifiedName~ResilienceTests"

# Run specific test
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug --filter "TestId=RESILIENCE-001"
```

### With Custom App Path

```bash
# Windows
set MAUI_APP_PATH=C:\Custom\Path\To\IdeaBranch.App.exe
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug

# Or via command line (PowerShell)
$env:MAUI_APP_PATH = "C:\Custom\Path\To\IdeaBranch.App.exe"
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
```

### Debugging Tests

1. **Attach debugger**:
   - Set breakpoints in test code
   - Run tests in Debug mode
   - Use Visual Studio Test Explorer or VS Code debugger

2. **View Appium logs**:
   - WinAppDriver logs are printed to console
   - Check Windows Event Viewer for WinAppDriver events

3. **Take screenshots**:
   Tests can take screenshots using `AppiumHelpers.TakeScreenshot()`:

   ```csharp
   Driver!.TakeScreenshot(@"C:\Temp\test-screenshot.png");
   ```

## Troubleshooting

### Common Issues

#### 1. WinAppDriver Not Starting

**Symptoms**: Tests fail with connection errors.

**Solutions**:
- Verify WinAppDriver is running: `curl http://127.0.0.1:4723/status`
- Check Windows Firewall settings
- Ensure Developer Mode is enabled
- Try running WinAppDriver with administrator privileges

#### 2. App Not Launching

**Symptoms**: Tests fail with "app not found" errors.

**Solutions**:
- Verify `MAUI_APP_PATH` points to correct executable
- Ensure app is built successfully
- Check app path has no spaces or special characters
- Verify app executable exists and is accessible

#### 3. Elements Not Found

**Symptoms**: Tests fail with `NoSuchElementException`.

**Solutions**:
- Verify `AutomationId` values are set in XAML
- Check element is visible (not hidden or collapsed)
- Increase wait timeouts in test code
- Use `AppiumHelpers.TryFindElementByAutomationId()` for optional elements

#### 4. Timeout Issues

**Symptoms**: Tests timeout waiting for elements.

**Solutions**:
- Increase timeout values in test code:
  ```csharp
  Driver!.WaitForElementVisible("MyElement", TimeSpan.FromSeconds(30));
  ```
- Verify app is responsive (not frozen)
- Check network connectivity if tests involve API calls

#### 5. Multiple App Instances

**Symptoms**: Tests interfere with each other.

**Solutions**:
- Ensure each test closes app properly in `TearDown`
- Use `Driver.Quit()` to close app between tests
- Kill any existing app instances before running tests

### Debug Mode

Enable verbose logging:

```bash
# Enable detailed test output
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug -v detailed

# Enable Appium logging
# Set environment variable
set APPIUM_LOG_LEVEL=debug
```

### Getting Help

1. Check WinAppDriver logs in console output
2. Review test output for specific error messages
3. Verify all prerequisites are installed correctly
4. Check `.github/workflows/` for CI configuration examples

## CI/CD Integration

### GitHub Actions (Windows)

Example workflow:

```yaml
name: UI Tests

on: [push, pull_request]

jobs:
  ui-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Enable Developer Mode
        run: |
          reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock" /t REG_DWORD /f /v "AllowDevelopmentWithoutDevLicense" /d "1"
      
      - name: Install WinAppDriver
        run: |
          # Download and install WinAppDriver
          # (Add installation steps here)
      
      - name: Build App
        run: |
          dotnet build src/IdeaBranch.App/IdeaBranch.App.csproj -c Release -f net9.0-windows10.0.19041.0
      
      - name: Start WinAppDriver
        run: |
          Start-Process -FilePath "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe" -WindowStyle Hidden
          Start-Sleep -Seconds 5
      
      - name: Run UI Tests
        run: |
          dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Release --logger "trx;LogFileName=test-results.trx"
        env:
          MAUI_APP_PATH: ${{ github.workspace }}\src\IdeaBranch.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe
      
      - name: Publish Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: test-results.trx
```

### Azure DevOps (Windows)

Example pipeline:

```yaml
pool:
  vmImage: 'windows-latest'

steps:
  - task: DotNetCoreCLI@2
    displayName: 'Build App'
    inputs:
      command: 'build'
      projects: 'src/IdeaBranch.App/IdeaBranch.App.csproj'
      arguments: '-c Release -f net9.0-windows10.0.19041.0'
  
  - script: |
      Start-Process -FilePath "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe" -WindowStyle Hidden
      Start-Sleep -Seconds 5
    displayName: 'Start WinAppDriver'
  
  - task: DotNetCoreCLI@2
    displayName: 'Run UI Tests'
    inputs:
      command: 'test'
      projects: 'tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj'
      arguments: '-c Release'
    env:
      MAUI_APP_PATH: '$(Build.SourcesDirectory)\src\IdeaBranch.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\IdeaBranch.App.exe'
  
  - task: PublishTestResults@2
    displayName: 'Publish Test Results'
    inputs:
      testResultsFiles: '**/*.trx'
```

## Additional Resources

- [WinAppDriver GitHub](https://github.com/microsoft/WinAppDriver)
- [Appium Documentation](https://appium.io/docs/en/latest/)
- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Appium.WebDriver NuGet Package](https://www.nuget.org/packages/Appium.WebDriver/)

