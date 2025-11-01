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

