# platforms Specification

## Purpose
TBD - created by archiving change add-initial-idea-branch-specs. Update Purpose after archive.
## Requirements
### Requirement: Application supports Windows and iOS via .NET MAUI
The system SHALL build and run on Windows (WinUI) and iOS using .NET MAUI LTS.

#### Scenario: Windows build succeeds
- **WHEN** building the solution in Release for WinUI
- **THEN** the app produces a runnable MSIX artifact without errors

#### Scenario: iOS build succeeds
- **WHEN** building the solution in Release for iOS
- **THEN** the app produces an IPA artifact suitable for TestFlight

### Requirement: Latest C# Language Version
All .NET MAUI projects MUST compile with the latest stable C# language version supported by the selected .NET SDK.

#### Scenario: Project configuration
- **WHEN** creating or updating a MAUI project
- **THEN** `LangVersion` is set to `latest` (preferably via a shared `Directory.Build.props`) to enable current C# features

### Requirement: Compatibility with XAML-first architecture
.NET MAUI project templates and guidance MUST support XAML-based pages and controls with C# code-behind and/or MVVM.

#### Scenario: New solution scaffolding
- **WHEN** scaffolding a new solution
- **THEN** the default project structure supports XAML pages (`.xaml` + `.xaml.cs`) and viewmodels bound via data binding

