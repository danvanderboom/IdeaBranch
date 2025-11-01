## ADDED Requirements
### Requirement: Interactive SkiaSharp timeline rendering
The system SHALL render the Timeline view using a SkiaSharp-based canvas for high-performance drawing and interaction.

#### Scenario: Render dense datasets smoothly
- **WHEN** ≥50k timeline events are displayed
- **THEN** the timeline renders at ≥55 FPS on typical hardware
- **AND** memory usage remains stable without leaks

### Requirement: Scroll and zoom timeline axis
The system SHALL support horizontal and vertical orientations, panning/scrolling, and pinch/scroll zoom across day/week/month/year scales.

#### Scenario: Zoom between scales
- **WHEN** the user pinch-zooms or uses zoom controls
- **THEN** the timeline re-scales (day→week→month→year)
- **AND** event positions remain correct

### Requirement: Precision-aware rendering
The system SHALL render events with variable temporal precision (year, month, day) and durations (start/end) accurately.

#### Scenario: Render imprecise dates
- **WHEN** an event has only a year precision
- **THEN** it is positioned and sized appropriately for that scale

### Requirement: Event markers sized and colored by type
The system SHALL style event markers by type/category with configurable size and color mapping and show a legend.

#### Scenario: Visual encoding by type
- **WHEN** multiple event types are displayed
- **THEN** each type uses its configured color/size
- **AND** a legend explains the mapping

### Requirement: Click/tap for event details
The system SHALL support hit-testing to select an event and open its details panel or navigate to the associated node.

#### Scenario: Select event for details
- **WHEN** the user taps/clicks an event marker
- **THEN** the event is highlighted
- **AND** details are shown via the app's details view

### Requirement: Range selection for filtering
The system SHALL support drag-to-select a time range to filter timeline content and propagate filters to other views.

#### Scenario: Brush select range
- **WHEN** the user drags to create a time brush
- **THEN** only events within the range are displayed
- **AND** the range is exposed to filtering pipelines

### Requirement: Event clustering in dense periods
The system SHALL cluster overlapping events based on pixel density, expanding as users zoom in.

#### Scenario: Expand clusters on zoom
- **WHEN** the user zooms into a clustered region
- **THEN** clusters split into individual events progressively

