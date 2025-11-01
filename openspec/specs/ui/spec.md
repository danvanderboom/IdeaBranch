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

### Requirement: Map view displays geospatial annotations
The system SHALL display geospatial annotations on the Map view, showing locations and regions associated with topic nodes.

#### Scenario: Display geospatial annotations
- **WHEN** the Map view is opened
- **THEN** geospatial annotations from topic nodes are displayed on the map
- **AND** locations are represented as markers or regions

#### Scenario: Annotations linked to topic nodes
- **WHEN** a geospatial annotation is displayed on the map
- **THEN** users can interact with the annotation to navigate to the associated topic node
- **AND** the relationship between annotation and node is clear

### Requirement: Interactive map controls
The system SHALL provide interactive map controls for zooming, panning, and selecting regions or points of interest.

#### Scenario: Zoom map
- **WHEN** a user zooms the map
- **THEN** the map view zooms in or out
- **AND** geospatial annotations remain correctly positioned

#### Scenario: Pan map
- **WHEN** a user pans the map
- **THEN** the map view moves to show different regions
- **AND** geospatial annotations remain correctly positioned

#### Scenario: Select location
- **WHEN** a user selects a location or region on the map
- **THEN** annotations or topic nodes associated with that location are highlighted or shown

### Requirement: Map view filtering by tags
The system SHALL allow users to filter geospatial data displayed on the Map view using tags from the hierarchical tag taxonomy.

#### Scenario: Filter by single tag
- **WHEN** a user applies a tag filter on the Map view
- **THEN** only geospatial annotations with the selected tag are displayed
- **AND** other annotations are hidden

#### Scenario: Filter by hierarchical tags
- **WHEN** a user applies a high-level tag filter
- **THEN** geospatial annotations associated with that tag and its child tags are displayed
- **AND** users can apply filters at different levels of granularity

### Requirement: Map view filtering by temporal data
The system SHALL allow users to filter geospatial data on the Map view by temporal information (dates, time periods).

#### Scenario: Filter by time period
- **WHEN** a user applies a temporal filter on the Map view
- **THEN** only geospatial annotations with temporal data matching the filter are displayed
- **AND** the map updates to show only relevant locations for the selected time period

### Requirement: Map view filtering by topic node
The system SHALL allow users to filter geospatial data based on the currently selected topic node or a topic node and its descendants.

#### Scenario: Filter by selected node
- **WHEN** a user selects a topic node in the topic tree
- **THEN** the Map view displays only geospatial annotations from that node and its descendants
- **AND** the map updates to reflect the filtered data

#### Scenario: Filter by node subtree
- **WHEN** a user selects a topic node and its descendants
- **THEN** the Map view displays geospatial annotations from all nodes in the subtree
- **AND** the map shows the spatial distribution of the selected topic area

### Requirement: Map view multiple layers
The system SHALL allow users to toggle different map layers on and off to visualize different aspects of geospatial data.

#### Scenario: Toggle map layer
- **WHEN** a user toggles a map layer on or off
- **THEN** the layer's geospatial data is shown or hidden
- **AND** multiple layers can be active simultaneously

#### Scenario: Layer based on tags
- **WHEN** a user creates a map layer based on specific tags
- **THEN** only geospatial annotations with those tags are shown in that layer
- **AND** the layer can be toggled independently of other layers

### Requirement: Map view heatmaps
The system SHALL support heatmap visualization to show density or intensity of geospatial data.

#### Scenario: Display heatmap
- **WHEN** a user enables heatmap visualization on the Map view
- **THEN** the map displays a heatmap showing density or intensity of geospatial annotations
- **AND** color intensity represents the concentration of data points

### Requirement: Map view clustering
The system SHALL support clustering of nearby geospatial annotations to improve map readability.

#### Scenario: Cluster nearby annotations
- **WHEN** multiple geospatial annotations are close together
- **THEN** the map clusters them into a single marker
- **AND** users can zoom in to see individual annotations within the cluster

### Requirement: Timeline view displays temporal annotations
The system SHALL display temporal annotations on the Timeline view, showing sequences of events and time-based information.

#### Scenario: Display temporal annotations
- **WHEN** the Timeline view is opened
- **THEN** temporal annotations from topic nodes are displayed on the timeline
- **AND** events are organized chronologically

#### Scenario: Annotations linked to topic nodes
- **WHEN** a temporal annotation is displayed on the timeline
- **THEN** users can interact with the annotation to navigate to the associated topic node
- **AND** the relationship between annotation and node is clear

### Requirement: Chronological event visualization
The system SHALL display events and temporal data in chronological order on the Timeline view.

#### Scenario: Chronological display
- **WHEN** temporal annotations are displayed
- **THEN** events are arranged chronologically along the timeline
- **AND** the timeline shows a clear progression through time

#### Scenario: Multiple time scales
- **WHEN** users zoom in or out on the timeline
- **THEN** the timeline adjusts to show appropriate time scales (e.g., days, months, years, decades)
- **AND** events remain correctly positioned

### Requirement: Timeline view filtering by tags
The system SHALL allow users to filter temporal data displayed on the Timeline view using hierarchical tags.

#### Scenario: Filter by hierarchical tags
- **WHEN** a user applies tag filters on the Timeline view
- **THEN** only temporal annotations with matching tags are displayed
- **AND** users can apply filters at different levels of tag hierarchy granularity

### Requirement: Timeline view filtering by topic node
The system SHALL allow users to filter temporal data based on the currently selected topic node or a topic node and its descendants.

#### Scenario: Filter by selected node
- **WHEN** a user selects a topic node in the topic tree
- **THEN** the Timeline view displays only temporal annotations from that node and its descendants
- **AND** the timeline updates to reflect the filtered data

### Requirement: Dynamic timeline updates
The system SHALL automatically update the timeline as users gather dates and time periods in descendant nodes.

#### Scenario: Auto-update timeline
- **WHEN** a user adds temporal data to a node in a timeline subtree
- **THEN** the timeline automatically updates to include the new temporal information
- **AND** the timeline reflects the latest research findings

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

