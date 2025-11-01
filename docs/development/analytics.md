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

1. Navigate to the **Timeline** page (replaces the previous placeholder)
2. Select the source types you want to visualize
3. Configure filters:
   - **Grouping**: Choose how to group events (Day, Week, Month)
   - **Date Range**: Optional start and end date filters
   - **Tag Filters**: Select tags to filter content
4. Click **Generate Timeline** to create the visualization
5. Export results as:
   - **JSON**: Structured data format
   - **CSV**: Comma-separated values for spreadsheet applications
   - **PNG**: Visual image of the timeline

#### Event Types

- **Topic Created**: When a topic node was created
- **Topic Updated**: When a topic node was modified
- **Annotation Created**: When an annotation was created
- **Conversation Message**: When a prompt or response was added

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
- **Metadata**: Includes total event count, earliest and latest event timestamps

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

- SkiaSharp-based custom controls for richer visualizations
- Interactive word cloud (click words to filter/analyze)
- Timeline zoom controls
- Custom date range presets
- Export to additional formats (Excel, PDF)

