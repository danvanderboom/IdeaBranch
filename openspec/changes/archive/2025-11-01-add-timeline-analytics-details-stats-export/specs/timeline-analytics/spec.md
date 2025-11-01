## ADDED Requirements

### Requirement: Expandable Event Cards with Details (Analytics Timeline)
The system SHALL display events as expandable cards showing full details without leaving the Analytics timeline.
- Details SHALL include: title, description/body, event type, actor, source/service, tags (paths), start/end timestamps with precision, related node(s), related annotation(s).
- Users SHALL be able to expand/collapse a card inline or in a details drawer.

#### Scenario: Expand event card to view full details
- **WHEN** a user selects an event on the Analytics timeline
- **THEN** an expandable card opens with full event details
- **AND** the event is visually highlighted on the timeline

#### Scenario: Collapse event card
- **WHEN** a user closes the details
- **THEN** the card collapses and the timeline focus remains unchanged

### Requirement: Links to Related Nodes and Annotations
The system SHALL provide navigable links from an event's details to its related topic node(s) and annotation(s).
- Node links SHALL navigate to the node view while preserving current filters in session state.
- Annotation links SHALL open the annotation in its viewer/panel and allow navigation back.

#### Scenario: Navigate to related node
- **WHEN** a user clicks a related node link in the event card
- **THEN** the app navigates to the node view for that node
- **AND** a back/return affordance returns to the Analytics timeline context

#### Scenario: Open related annotation
- **WHEN** a user clicks a related annotation link
- **THEN** the annotation opens in its viewer/panel with relevant context

### Requirement: Group Events by Type into Bands
The system SHALL support grouping events by type into horizontal bands within the Analytics timeline.
- A "Group by type" toggle SHALL arrange events in separate labeled bands per event type.
- Band styling SHALL remain consistent with the type color/legend and clustering.

#### Scenario: Toggle group-by-type bands
- **WHEN** a user enables Group by type
- **THEN** events are arranged into separate horizontal bands per type with labels and legend

#### Scenario: Bands persist through zoom/cluster
- **WHEN** the user zooms or clusters expand/split
- **THEN** events remain within their type band and retain correct positions

### Requirement: Event Statistics (Counts per Type and Trends)
The system SHALL display a statistics module reflecting the currently filtered/visible Analytics timeline data.
- The module SHALL show per-type counts and a time-series trend (auto-binned by day/week/month based on the visible range).
- Interacting with a stat item SHALL highlight or filter the corresponding events.

#### Scenario: Stats update with filters and range
- **WHEN** filters or the visible time range change
- **THEN** per-type counts and trend charts update to reflect the current subset

#### Scenario: Click stat to highlight/filter
- **WHEN** a user clicks a type in the stats module
- **THEN** that type's events are highlighted
- **AND** optionally filtered if the user enables the filter mode

### Requirement: Export Filtered Subsets (CSV and JSON)
The system SHALL export the currently filtered subset of Analytics timeline events to CSV and JSON.
- Exports SHALL respect all active filters, search terms, and date/range selections.
- CSV columns SHALL include at minimum: `eventId`, `type`, `title`, `body`, `start`, `end`, `precision`, `nodeId`, `nodePath`, `tags`, `annotationIds`, `source`, `actor`, `createdAt`, `updatedAt`.
- JSON export SHALL include the same fields as objects in an array.
- Export processes SHALL be non-blocking and show progress/confirmation.

#### Scenario: Export CSV with filters applied
- **WHEN** a user chooses Export → CSV
- **THEN** a CSV file is generated containing only events matching current filters

#### Scenario: Export JSON with filters applied
- **WHEN** a user chooses Export → JSON
- **THEN** a JSON file is generated containing only events matching current filters

