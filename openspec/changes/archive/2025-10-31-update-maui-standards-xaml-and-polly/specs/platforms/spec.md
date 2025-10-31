## ADDED Requirements

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

