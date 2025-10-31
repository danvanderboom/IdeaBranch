## ADDED Requirements

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

