# ui Specification

## Purpose
TBD - created by archiving change add-initial-idea-branch-specs. Update Purpose after archive.
## Requirements
### Requirement: App UI exposes stable AutomationIds for testing
The system MUST expose stable `AutomationId` values for interactive UI elements on critical flows.

#### Scenario: Primary navigation AutomationIds exist
- **WHEN** the app renders the main shell/navigation
- **THEN** each primary navigation element has a unique, stable `AutomationId`

### Requirement: Primary navigation exposes core views
The system SHALL provide navigation to core views including Topic Tree, Map, and Timeline.

#### Scenario: Navigate to Topic Tree
- **WHEN** the user selects the Topic Tree nav item
- **THEN** the Topic Tree view is displayed

#### Scenario: Navigate to Map
- **WHEN** the user selects the Map nav item
- **THEN** the Map view is displayed

#### Scenario: Navigate to Timeline
- **WHEN** the user selects the Timeline nav item
- **THEN** the Timeline view is displayed

### Requirement: XAML-First UI Definition
UI for .NET MAUI apps SHALL be defined in XAML files, with behavior implemented in C# code-behind and/or MVVM viewmodels. Custom controls and reusable components SHOULD be authored as XAML-based `ContentView`s, `ControlTemplate`s, or `DataTemplate`s with bindable properties.

#### Scenario: New page is implemented
- **WHEN** a new screen is created in a .NET MAUI app
- **THEN** the visual tree is declared in a `.xaml` file
- **AND** supporting logic resides in the corresponding `.xaml.cs` and/or a viewmodel

#### Scenario: Reusable UI component
- **WHEN** a reusable UI component is needed
- **THEN** it is authored in XAML (e.g., `ContentView`) with bindable properties for data/context
- **AND** minimal imperative UI construction is performed in C# (reserved for dynamic/edge cases)

