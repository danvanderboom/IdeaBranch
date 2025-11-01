# timeline-analytics Specification

## Purpose
TBD - created by archiving change add-visualization-export-svg-png. Update Purpose after archive.
## Requirements
### Requirement: Analytics Timeline event connections
The system SHALL display connections between related events on the Analytics Timeline visualization.

- Connections SHALL be displayed as lines linking events that are related (e.g., cause-effect, sequence, semantic relationships, or shared annotations/nodes).
- Connections SHALL be visible within type bands and across type bands when event grouping by type is enabled.
- Users SHALL be able to toggle connections visibility on and off independently of event grouping.
- Connection rendering SHALL use routing algorithms to minimize overlaps and visual clutter (orthogonal or curved paths).
- Connection lines SHALL be styled with colors, line weights, and styles to indicate relationship types if applicable.
- A legend SHALL indicate connection styles and relationship types if multiple types are supported.
- Connections SHALL respect active filters and only show connections between currently visible events.

#### Scenario: Display connections between related events
- **WHEN** connections are enabled on the Analytics Timeline
- **THEN** lines are drawn between events that have defined relationships
- **AND** connections are routed appropriately to avoid overlaps with events and bands

#### Scenario: Toggle connections visibility
- **WHEN** a user toggles connections visibility off
- **THEN** connection lines are hidden but events remain visible
- **AND** the timeline layout and banding remain unchanged

#### Scenario: Connections within and across type bands
- **WHEN** event grouping by type is enabled with separate bands per type
- **THEN** connections can be displayed both within a band and across different type bands
- **AND** connections are routed appropriately to cross band boundaries without visual interference

#### Scenario: Connections respect filters
- **WHEN** filters are applied to the Analytics Timeline and connections are enabled
- **THEN** only connections between currently visible events are displayed
- **AND** connections to filtered-out events are hidden

#### Scenario: Connections legend
- **WHEN** multiple relationship types are shown with different connection styles
- **THEN** a legend indicates the meaning of each connection style (color, line weight, style)
- **AND** the legend is positioned appropriately and can be toggled

### Requirement: Analytics Timeline export to PNG and SVG
The system SHALL allow users to export the Analytics Timeline as PNG or SVG files with customizable quality settings.

- PNG exports SHALL support DPI scaling (1x-4x) and preserve banded layout, event positioning, connections, and statistics.
- SVG exports SHALL be vector format preserving all timeline styling, fonts, colors, connections, and visual elements.
- Exports SHALL include temporal annotations, type bands (if enabled), event connections (if visible), legends, and optionally statistics panels.
- Export processes SHALL respect all active filters, search terms, and date range selections.
- Export processes SHALL be non-blocking with progress indication.

#### Scenario: Export Analytics Timeline with banded layout
- **WHEN** a user exports the Analytics Timeline with event grouping by type enabled
- **THEN** the exported file preserves the horizontal bands grouping events by type
- **AND** band labels, legends, and type colors are preserved in the export

#### Scenario: Export Analytics Timeline with event connections
- **WHEN** a user exports the Analytics Timeline with event connections visible
- **THEN** the exported file includes connection lines between related events
- **AND** connections are rendered clearly with appropriate routing to avoid overlaps

#### Scenario: Export Analytics Timeline to high-DPI PNG
- **WHEN** a user exports the Analytics Timeline to PNG with 2x DPI setting
- **THEN** the PNG is generated at 2x resolution (144 DPI equivalent)
- **AND** all events, bands, connections, labels, and legends are rendered at higher resolution

#### Scenario: Export Analytics Timeline with filters applied
- **WHEN** a user exports the Analytics Timeline with active filters (tags, date range, search terms)
- **THEN** the exported file contains only events matching the filters
- **AND** connections are included only between visible events

#### Scenario: Export Analytics Timeline to SVG with theme
- **WHEN** a user exports the Analytics Timeline to SVG with custom theme and fonts
- **THEN** the SVG file preserves all theme colors, fonts, and styling
- **AND** fonts are embedded or referenced via font-family

#### Scenario: Export Analytics Timeline with statistics panel
- **WHEN** a user exports the Analytics Timeline with statistics panel visible in viewport
- **THEN** the exported file includes the statistics panel showing per-type counts and trends
- **AND** statistics reflect the current filtered subset of events

### Requirement: Analytics Timeline theming and styling
The system SHALL provide customizable theming for Analytics Timeline visualizations including fonts and background customization.

- Font options SHALL allow users to select fonts for timeline labels, event text, and statistics from system fonts or custom font files.
- Background options SHALL include solid colors, gradients, and image backgrounds with opacity control.
- Theme changes SHALL apply immediately to the Analytics Timeline visualization and be preserved in exports.

#### Scenario: Change Analytics Timeline fonts
- **WHEN** a user selects custom fonts for Analytics Timeline labels, events, and statistics
- **THEN** all text elements on the timeline use the selected fonts
- **AND** the fonts are preserved in PNG and SVG exports

#### Scenario: Set Analytics Timeline background
- **WHEN** a user selects a background (solid color, gradient, or image) for the Analytics Timeline
- **THEN** the timeline displays the selected background
- **AND** the background is preserved in exports

#### Scenario: Apply theme to Analytics Timeline
- **WHEN** a user applies a predefined theme to the Analytics Timeline
- **THEN** event colors, band colors, connection colors, and overall styling match the theme
- **AND** the theme is preserved in exports

### Requirement: Hierarchical Tag Filtering
The system SHALL provide a hierarchical tag picker that supports parent/child tags.
- Users SHALL be able to select any tag node.
- For any selected parent tag, the system SHALL provide a per-tag "Include descendants" toggle (default OFF).
- The Tags facet SHALL use OR semantics within the facet and combine with other facets using AND.

#### Scenario: Select parent with descendants OFF
- **WHEN** the user selects tag "A" with Include descendants OFF
- **THEN** results include events tagged with exactly "A" (not children)

#### Scenario: Select parent with descendants ON
- **WHEN** the user toggles Include descendants ON for tag "A"
- **THEN** results include events tagged with "A" OR any descendant tag of "A"

#### Scenario: Multiple tags selected (OR within facet)
- **WHEN** the user selects tags "A" and "B"
- **THEN** results include events tagged with "A" OR "B"

### Requirement: Event Type Filtering (Created/Updated)
The system SHALL allow filtering by event types: Created, Updated.
- Default state SHALL have both types active (no narrowing).
- The Event Type facet SHALL use OR semantics within the facet and combine with other facets using AND.

#### Scenario: Show only Created
- **WHEN** the user selects only "Created"
- **THEN** only Created-type events are shown

#### Scenario: Created OR Updated (default)
- **WHEN** both types are selected
- **THEN** no filtering is applied by event type

### Requirement: Quick Date Presets
The system SHALL provide quick presets for date range: Last 7 days, This month, This year.
- Last 7 days: rolling window [now-7d, now]
- This month: [start of current calendar month 00:00, now] in the user's timezone
- This year: [Jan 1 00:00 of current year, now] in the user's timezone
- Selecting a preset SHALL update the date range control; switching to a custom range SHALL clear the preset label.

#### Scenario: Apply Last 7 days
- **WHEN** the user selects "Last 7 days"
- **THEN** only events within the last 7×24 hours up to now are shown

#### Scenario: Apply This month preset
- **WHEN** the user selects "This month"
- **THEN** only events from the start of the current calendar month (00:00 in user timezone) to now are shown

#### Scenario: Apply This year preset
- **WHEN** the user selects "This year"
- **THEN** only events from January 1 00:00 of the current year (in user timezone) to now are shown

#### Scenario: Custom range clears preset
- **WHEN** the user manually adjusts the date range after selecting a preset
- **THEN** the preset label is cleared

### Requirement: Free-Text Search Within Timeline
The system SHALL provide a search input that filters events using case-insensitive substring matching.
- Search scope SHALL include: event title, body/message, tag names/paths, source/service name, and actor display name.
- Minimum query length SHALL be 2 characters.
- The Search facet SHALL combine with other facets using AND.

#### Scenario: Phrase match
- **WHEN** the user enters "policy update"
- **THEN** events whose searchable text contains the phrase "policy update" are shown

#### Scenario: Minimum query length
- **WHEN** the user enters a single character
- **THEN** no search filtering is applied

#### Scenario: Case-insensitive search
- **WHEN** the user enters "POLICY"
- **THEN** events matching "policy", "Policy", "POLICY", etc. are shown

### Requirement: Filter Combination Logic (Faceted Boolean)
The system SHALL apply AND across different facets and OR within a facet for multi-select enumerations.
- Facets include: Date range/preset, Source type, Tags, Event type, Search.
- Clearing a facet SHALL remove its constraint from the query.

#### Scenario: Combine tags + event type + date
- **WHEN** the user selects tags A or B; selects event type Created; selects preset Last 7 days
- **THEN** results satisfy (A OR B) AND Created AND within Last 7 days

#### Scenario: Combine search + tags
- **WHEN** the user enters search query "update" and selects tag "A"
- **THEN** results satisfy (search matches "update") AND (tag matches "A")

#### Scenario: Clear facet removes constraint
- **WHEN** the user clears all tag selections
- **THEN** the Tags facet constraint is removed from the query

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

