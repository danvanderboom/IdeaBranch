## ADDED Requirements

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

