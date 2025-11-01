# Analytics Features

## Overview

The IdeaBranch application provides analytics capabilities for visualizing and analyzing content through word clouds and timeline visualizations.

## Features

### Word Cloud Analytics

Generate word clouds from your content to visualize the most frequent words across:

- **Prompts**: Text from topic prompts
- **Responses**: Text from topic responses
- **Annotations**: Comment text from annotations
- **Topics**: Combined prompt and response text

#### Usage

1. Navigate to the **Word Cloud** page from the app shell
2. Select the source types you want to analyze (Prompts, Responses, Annotations, Topics)
3. Configure filters:
   - **Min Frequency**: Minimum word count threshold (default: 1)
   - **Max Words**: Maximum number of words to display (default: 100)
   - **Date Range**: Optional start and end date filters
   - **Tag Filters**: Select tags to filter content (with hierarchical support)
4. Click **Generate Word Cloud** to create the visualization
5. Configure visualization:
   - **Layout**: Random (default), Spiral, or Force-Directed
   - **Theme**: Palettes, gradient color mapping, custom font-family
   - **Background**: Solid color, transparent
   - **DPI**: 1x–4x for high-resolution output
6. Export results as:
   - **JSON**: Structured data format
   - **CSV**: Comma-separated values for spreadsheet applications
   - **PNG**: Visual image (respects DPI, theme, background)
   - **SVG**: Vector output (preserves fonts and styling)

#### Filtering

- **Tag-based filtering**: Select one or more tags to include only content associated with those tags
- **Hierarchical tags**: Enable "Include Descendants" to include content tagged with child tags
- **Date filtering**: Set start and end dates to analyze content from specific time periods

### Timeline Analytics

Generate timeline visualizations to see chronological events:

- **Topics**: Topic creation and update events
- **Annotations**: Annotation creation events
- **Conversations**: Conversation message events

#### Usage

1. Navigate to the **Timeline** page from the app shell
2. Select the source types you want to visualize (Topics, Annotations, Conversations)
3. Configure filters:
   - **Event Types**: Filter by Created events, Updated events, or both (default: both)
   - **Search**: Free-text search within event titles, bodies, tag names, source/service names, and actor display names (minimum 2 characters)
   - **Tags**: Hierarchical tag picker with per-tag "Include descendants" toggle (default: OFF for each tag)
   - **Date Range**: 
     - Quick presets: "Last 7 days", "This month", "This year"
     - Custom date range: Optional start and end date filters
   - **Grouping**: Choose how to group events (Day, Week, Month)
4. Click **Generate Timeline** to create the visualization
5. **Interact with events**:
   - Click on an event marker to view full details in an expandable card
   - Navigate to related topic nodes or annotations directly from event details
   - Toggle "Group by Type" to organize events into horizontal bands by event type
6. **View statistics**:
   - Per-type event counts automatically update with filters
   - Time-series trend sparklines show event frequency over time (auto-binned by day/week/month based on date range)
   - Click on stat items to highlight corresponding events on the timeline
7. Connections overlay (optional):
   - Show lines between related events within/across bands
   - Routed to reduce overlaps; included in exports when enabled
8. Export results as:
   - **JSON**: Structured data format with all event fields (respects all active filters)
   - **CSV**: Comma-separated values with all event fields (respects all active filters)
   - **PNG**: Visual image (respects DPI, theme)
   - **SVG**: Vector output (preserves bands, connections, fonts)

#### Event Types

- **Created Events**: 
  - Topic Created: When a topic node was created
  - Annotation Created: When an annotation was created
  - Conversation Message: When a prompt or response was added
- **Updated Events**:
  - Topic Updated: When a topic node was modified
  - Annotation Updated: When an annotation was modified

#### Event Details

When you click on an event marker, an expandable details card appears showing:

- **Event Information**: Title, description/body, event type, actor, source/service
- **Timestamps**: Start/end timestamps with precision information
- **Tags**: All tags associated with the event (displayed as formatted paths)
- **Related Content**: Navigation links to:
  - **Related Topic Nodes**: Navigate to the topic node while preserving current filters
  - **Related Annotations**: Open the annotation in its editor/viewer with return-to-context navigation

#### Grouping by Type

- **Group by Type Toggle**: Organize events into horizontal bands labeled by event type
- **Type Bands**: Each event type gets its own labeled band with consistent styling
- **Persistent Bands**: Bands remain organized correctly through zoom and clustering operations
- **Visual Organization**: Improves readability when multiple event types are present

#### Statistics Module

The statistics module provides real-time insights into the filtered event data:

- **Per-Type Counts**: Shows the number of events for each event type in the current filtered subset
- **Time-Series Trends**: Sparkline visualizations showing event frequency over time
  - **Auto-Binning**: Automatically selects binning granularity based on visible date range:
    - **Day binning**: For ranges less than 3 months
    - **Week binning**: For ranges 3 months to 2 years
    - **Month binning**: For ranges 2 years or more
- **Interactive Highlighting**: Click on any stat item to highlight the corresponding event type on the timeline
- **Auto-Update**: Statistics automatically update when filters or date ranges change

#### Advanced Filtering

Timeline Analytics supports powerful faceted filtering with boolean logic:

- **Faceted Boolean Logic**: 
  - **AND across facets**: All selected facets must be satisfied
  - **OR within facets**: Multiple selections within the same facet use OR logic
  - Example: (Tag A OR Tag B) AND Created AND (Last 7 days) AND (search matches "policy")

- **Hierarchical Tag Filtering**:
  - Select any tag in the hierarchy
  - Per-tag toggle: "Include descendants" (default: OFF)
  - When enabled for a parent tag, includes all child/descendant tags
  - Multiple tags selected use OR logic within the Tags facet

- **Event Type Filtering**:
  - Filter by Created events only, Updated events only, or both (default: both)
  - Both types selected means no filtering is applied by event type

- **Quick Date Presets**:
  - **Last 7 days**: Rolling 7-day window from now
  - **This month**: From start of current calendar month to now (user timezone)
  - **This year**: From January 1 of current year to now (user timezone)
  - Custom range: Manually set start and end dates (clears preset when adjusted)

- **Free-Text Search**:
  - Case-insensitive substring matching
  - Minimum query length: 2 characters
  - Searches in: event title, body/message, tag names/paths, source/service name, actor display name
  - Queries shorter than 2 characters are ignored

## Data Processing

### Word Cloud

- **Tokenization**: Text is split into words, normalized to lowercase
- **Stop Word Removal**: Common English stop words are filtered out
- **Frequency Counting**: Words are counted and sorted by frequency
- **Weight Calculation**: Word frequencies are normalized to 0.0-1.0 for visualization
- **Filtering**: Words below the minimum frequency threshold are excluded

### Timeline

- **Chronological Ordering**: Events are sorted by timestamp
- **Grouping**: Events are grouped into bands by time period (day, week, or month)
- **Filtering**: Events are filtered using faceted boolean logic:
  - Event type filtering (Created/Updated)
  - Tag filtering with hierarchical support (per-tag descendant inclusion)
  - Date range filtering (presets or custom)
  - Free-text search across event metadata (including tag names/paths)
  - All filters combined with AND logic across facets, OR logic within facets
- **Metadata**: Includes total event count, earliest and latest event timestamps, and applied filters
- **Performance**: Query execution time and event reduction percentage are logged for performance monitoring

### Map Visualization

View geospatial overlays and export current viewport:

1. Place overlay points/labels
2. Optional tile grid (provider tiles may have licensing limits)
3. Theme/background options; DPI 1x–4x
4. Export:
   - **PNG**: Visual image (respects DPI, legend)
   - **SVG**: Vector output (labels, points, grid)

## How to Export Visualizations

### Exporting Word Cloud

1. Generate a word cloud using the filters and options described above
2. Once the word cloud is generated, locate the **Export** buttons at the bottom of the word cloud display
3. Choose your export format:
   - **Export JSON**: Exports word frequency data in JSON format for programmatic use
   - **Export CSV**: Exports word frequency data in CSV format for spreadsheet applications
   - **Export PNG**: Exports the word cloud as a high-quality PNG image (configurable DPI, theme, background)
   - **Export SVG**: Exports the word cloud as a scalable vector graphic (preserves fonts and styling)
4. For PNG/SVG exports, the following settings from **Settings > Export** are applied:
   - **DPI Scale**: 1x–4x for PNG resolution (1x = 72 DPI, 4x = 288 DPI)
   - **Background Color**: Solid color or transparent background
   - **Font Family**: Custom font family name (if set)
   - **Color Palette**: Predefined palette (vibrant, pastel, etc.)
   - **Word Cloud Layout**: Random, Spiral, or Force-Directed (configured in settings)
5. The exported file is saved to the app data directory and opened via the system share dialog

### Exporting Timeline

1. Generate a timeline using the filters and options described above
2. Once the timeline is generated, locate the **Export** buttons at the bottom of the timeline display
3. Choose your export format:
   - **Export JSON**: Exports timeline event data in JSON format with all required fields
   - **Export CSV**: Exports timeline event data in CSV format with all required fields
   - **Export PNG**: Exports the timeline as a high-quality PNG image (configurable DPI, theme, connections, statistics)
   - **Export SVG**: Exports the timeline as a scalable vector graphic (preserves bands, connections, fonts)
4. For PNG/SVG exports, the following settings are applied:
   - **DPI Scale**: 1x–4x for PNG resolution
   - **Background Color**: Solid color or transparent background
   - **Font Family**: Custom font family name (if set)
   - **Color Palette**: Predefined palette
   - **Include Connections**: Whether to show event-to-event connection lines (configured in settings)
   - **Include Statistics**: Whether to include the statistics panel in the export (configured in settings)
5. Event connections are automatically generated for events with the same NodeId
6. The exported file is saved to the app data directory and opened via the system share dialog
7. Export progress is indicated by a loading spinner and "Exporting..." label

### Exporting Map

1. Navigate to the **Map** page from the app shell
2. The map view displays topic relationships visually
3. Locate the **Export** buttons at the top of the map page
4. Choose your export format:
   - **Export PNG**: Exports the map as a high-quality PNG image (configurable DPI, theme, tiles, legend)
   - **Export SVG**: Exports the map as a scalable vector graphic (preserves overlays, grid, labels)
5. For PNG/SVG exports, the following settings are applied:
   - **DPI Scale**: 1x–4x for PNG resolution
   - **Background Color**: Solid color or transparent background
   - **Font Family**: Custom font family name (if set)
   - **Color Palette**: Predefined palette
   - **Include Map Tiles**: Whether to include base map tiles (configured in settings)
   - **Include Map Legend**: Whether to include a legend (configured in settings)
6. The exported file is saved to the app data directory and opened via the system share dialog
7. Export progress is indicated by a loading spinner and "Exporting..." label

## Export Formats

### JSON

JSON export includes:
- Full word frequency data or timeline event data
- Timeline JSON includes all required fields: `eventId`, `type`, `title`, `body`, `start`, `end`, `precision`, `nodeId`, `nodePath`, `tags`, `annotationIds`, `source`, `actor`, `createdAt`, `updatedAt`
- Applied filters and options
- Metadata (generation timestamp, counts, etc.)
- **Respects all active filters**: Only exports events matching current filter criteria

### CSV

CSV export includes:
- Tabular data format
- Column headers with all required fields for timeline exports
- One row per word/event
- **Respects all active filters**: Only exports events matching current filter criteria
- Special characters (commas, quotes, newlines) are properly escaped

### PNG & SVG

PNG/SVG exports include:
- Visual representation of the data with theming and background options
- Word cloud: Layout (Random/Spiral/Force-Directed), gradient palettes, custom fonts
- Timeline: Banded layout, optional connections, optional statistics panel
- Map: Overlays, optional grid/legend
- DPI scaling: 1x–4x (applies to PNG); SVG is resolution-independent
- Progress indication during export operations
- All export settings are configurable in **Settings > Export**

## Architecture

### Services

- **WordCloudService**: Handles word cloud generation logic
- **TimelineService**: Handles timeline generation logic
- **AnalyticsService**: Coordinates both services through a unified interface
- **AnalyticsExportService**: Handles export to various formats

### Repositories

- **IConversationsRepository**: Accesses conversation data (prompts/responses)
- **IAnnotationsRepository**: Accesses annotation data
- **ITopicTreeRepository**: Accesses topic tree data
- **ITagTaxonomyRepository**: Accesses tag taxonomy for filtering

### ViewModels

- **WordCloudViewModel**: Manages word cloud page state and interactions
- **TimelineViewModel**: Manages timeline page state and interactions including:
  - Event selection and details display
  - Group-by-type toggle and state
  - Statistics calculation (per-type counts and trends)
  - Auto-binning logic for trend visualization
  - Navigation to related nodes and annotations
  - Export operations with progress indication

## Performance Considerations

- Word cloud generation processes text in batches
- Large datasets may take time to process
- Maximum word count limits help maintain performance
- Timeline grouping reduces memory usage for large event sets

## Future Enhancements

- SkiaSharp-based custom controls for richer visualizations (✅ Implemented)
- Interactive word cloud (click words to filter/analyze)
- Timeline zoom controls (✅ Implemented)
- Custom date range presets (✅ Implemented)
- Enhanced tag picker UI with hierarchical selection and per-tag descendant toggles (✅ Implemented)
- Expandable event details cards (✅ Implemented)
- Navigation links to related nodes and annotations (✅ Implemented)
- Group by type bands (✅ Implemented)
- Statistics module with auto-binning trends (✅ Implemented)
- Export to additional formats (Excel, PDF)

