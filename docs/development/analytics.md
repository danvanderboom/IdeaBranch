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
5. Export results as:
   - **JSON**: Structured data format
   - **CSV**: Comma-separated values for spreadsheet applications
   - **PNG**: Visual image of the word cloud

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
5. Export results as:
   - **JSON**: Structured data format
   - **CSV**: Comma-separated values for spreadsheet applications
   - **PNG**: Visual image of the timeline

#### Event Types

- **Created Events**: 
  - Topic Created: When a topic node was created
  - Annotation Created: When an annotation was created
  - Conversation Message: When a prompt or response was added
- **Updated Events**:
  - Topic Updated: When a topic node was modified
  - Annotation Updated: When an annotation was modified

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
  - Free-text search across event metadata
  - All filters combined with AND logic across facets, OR logic within facets
- **Metadata**: Includes total event count, earliest and latest event timestamps, and applied filters

## Export Formats

### JSON

JSON export includes:
- Full word frequency data or timeline event data
- Applied filters and options
- Metadata (generation timestamp, counts, etc.)

### CSV

CSV export includes:
- Tabular data format
- Column headers
- One row per word/event

### PNG

PNG export includes:
- Visual representation of the data
- Word cloud: Words sized by frequency
- Timeline: Chronological bands with event markers

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
- **TimelineViewModel**: Manages timeline page state and interactions

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
- Enhanced tag picker UI with hierarchical selection and per-tag descendant toggles
- Export to additional formats (Excel, PDF)

