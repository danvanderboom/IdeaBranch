## Development Setup (Windows and iOS)

### Common
- Install .NET 8 SDK (LTS)
- Verify workloads for .NET MAUI:
  ```bash
  dotnet workload install maui
  ```

### Windows
- OS: Windows 10/11
- Visual Studio 2022 with:
  - .NET Multi-platform App UI development
  - Windows app SDK (WinUI)
- MSIX Packaging Tools (for local sideload if needed)

### iOS (for future builds)
- macOS host with Xcode (latest supported by .NET MAUI LTS)
- Apple developer account, provisioning profiles, certificates
- Pair to Mac or CI runner for iOS builds

### Build commands (outline)
```bash
# Restore
dotnet restore

# Windows build
dotnet build -c Release -f net8.0-windows10.0.19041.0

# iOS build (example; adjust project and provisioning settings)
dotnet build -c Release -f net8.0-ios
```

### Notes
- Keep `AutomationId` stable for UI testability
- See `docs/testing/ui-automation.md` for UI tests

