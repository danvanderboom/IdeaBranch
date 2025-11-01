## ADDED Requirements

### Requirement: Map export to PNG and SVG
The system SHALL allow users to export the Map view as PNG or SVG files with customizable quality and options.

- PNG exports SHALL support DPI scaling (1x-4x) and respect all active filters and visible layers.
- SVG exports SHALL be vector format preserving map styling, annotations, and legends.
- Exports SHALL include visible map layers, annotations, labels, and optionally legends.
- Map tile content SHALL include appropriate licensing disclaimers or be excluded based on provider restrictions.
- Transparent backgrounds SHALL be supported for PNG exports.
- Export processes SHALL be non-blocking with progress indication.

#### Scenario: Export Map with all visible layers
- **WHEN** a user exports the Map view with multiple layers visible
- **THEN** the exported PNG/SVG includes all visible layers with their annotations and styling
- **AND** hidden layers are excluded from the export

#### Scenario: Export Map with filters applied
- **WHEN** a user exports the Map view with active tag and temporal filters
- **THEN** the exported file contains only geospatial annotations matching the filters
- **AND** the export respects the current map zoom level and viewport

#### Scenario: Export Map with transparent background
- **WHEN** a user selects transparent background for Map export
- **THEN** the exported PNG has an alpha channel with transparent background
- **AND** map tiles, annotations, and labels remain visible

#### Scenario: Export Map with legend included
- **WHEN** a user exports the Map view with legend option enabled
- **THEN** the exported file includes the legend showing layer types, colors, and symbols
- **AND** the legend is positioned appropriately in the export

### Requirement: Map theming and styling
The system SHALL provide customizable theming for Map views including fonts for labels and background customization.

- Font options SHALL allow users to select fonts for map labels from system fonts or custom font files.
- Background options SHALL include solid colors, gradients, and image backgrounds with opacity control.
- Theme changes SHALL apply immediately to the map visualization and be preserved in exports.

#### Scenario: Change map label fonts
- **WHEN** a user selects a custom font for map labels
- **THEN** all annotation labels and text elements on the map use the selected font
- **AND** the font is preserved in PNG and SVG exports

#### Scenario: Set map background to gradient
- **WHEN** a user selects a gradient background for the map
- **THEN** the map background displays the selected gradient
- **AND** the gradient is preserved in exports

#### Scenario: Apply image background with opacity
- **WHEN** a user selects an image background with opacity setting
- **THEN** the map displays the image background at the specified opacity level
- **AND** map tiles and annotations remain visible over the background

### Requirement: Timeline view export to PNG and SVG
The system SHALL allow users to export the Timeline view as PNG or SVG files with customizable quality settings.

- PNG exports SHALL support DPI scaling (1x-4x) and preserve the banded layout, event positioning, and connections.
- SVG exports SHALL be vector format preserving timeline styling, fonts, colors, and all visual elements.
- Exports SHALL include temporal annotations, bands (if enabled), connections (if enabled), and legends.
- Export processes SHALL be non-blocking with progress indication.

#### Scenario: Export Timeline with banded layout
- **WHEN** a user exports the Timeline view with banded layout enabled
- **THEN** the exported file preserves the horizontal bands grouping events by type
- **AND** band labels and legends are included if visible in the viewport

#### Scenario: Export Timeline with event connections
- **WHEN** a user exports the Timeline view with event connections visible
- **THEN** the exported file includes connection lines between related events
- **AND** connections are rendered clearly without overlap where possible

#### Scenario: Export Timeline to high-DPI PNG
- **WHEN** a user exports the Timeline to PNG with 2x DPI setting
- **THEN** the PNG is generated at 2x resolution (144 DPI equivalent)
- **AND** all events, bands, connections, and labels are rendered at higher resolution

### Requirement: Timeline view theming and styling
The system SHALL provide customizable theming for Timeline views including fonts and background customization.

- Font options SHALL allow users to select fonts for timeline labels and event text from system fonts or custom font files.
- Background options SHALL include solid colors, gradients, and image backgrounds with opacity control.
- Theme changes SHALL apply immediately to the timeline visualization and be preserved in exports.

#### Scenario: Change timeline fonts
- **WHEN** a user selects custom fonts for timeline labels and events
- **THEN** all text elements on the timeline use the selected fonts
- **AND** the fonts are preserved in PNG and SVG exports

#### Scenario: Set timeline background
- **WHEN** a user selects a background (solid color, gradient, or image) for the timeline
- **THEN** the timeline displays the selected background
- **AND** the background is preserved in exports

### Requirement: Timeline event connections
The system SHALL display connections between related events on the Timeline view.

- Connections SHALL be displayed as lines linking events that are related (e.g., cause-effect, sequence, or semantic relationships).
- Connections SHALL be visible within bands and across bands when event grouping by type is enabled.
- Users SHALL be able to toggle connections visibility on and off.
- Connection rendering SHALL use routing algorithms to minimize overlaps (orthogonal or curved paths).
- Connection lines SHALL be styled with colors and line weights to indicate relationship types if applicable.
- A legend SHALL indicate connection styles and relationship types if multiple types are supported.

#### Scenario: Display connections between related events
- **WHEN** connections are enabled on the Timeline view
- **THEN** lines are drawn between events that have defined relationships
- **AND** connections are rendered with appropriate routing to avoid overlaps

#### Scenario: Toggle connections visibility
- **WHEN** a user toggles connections visibility off
- **THEN** connection lines are hidden but events remain visible
- **AND** the timeline layout adjusts if needed

#### Scenario: Connections across event bands
- **WHEN** event grouping by type is enabled and connections are shown
- **THEN** connections can span across different type bands
- **AND** connections are routed appropriately to cross band boundaries

#### Scenario: Connections legend
- **WHEN** multiple relationship types are shown with different connection styles
- **THEN** a legend indicates the meaning of each connection style
- **AND** the legend is included in exports if visible

