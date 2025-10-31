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

