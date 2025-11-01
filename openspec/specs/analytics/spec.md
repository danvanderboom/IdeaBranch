# analytics Specification

## Purpose
TBD - created by archiving change add-analytics. Update Purpose after archive.
## Requirements
### Requirement: Word cloud generation
The system SHALL allow users to generate word clouds from prompts and responses in conversations with configurable layout options.

- Layout options SHALL include random layout (default), force-directed layout, and spiral layout.
- Force-directed layout SHALL arrange words using physics-based simulation with collision detection.
- Spiral layout SHALL arrange words along an expanding spiral path with collision resolution.
- Users SHALL be able to select layout type and adjust parameters (e.g., spiral tightness, force strength).

#### Scenario: Generate word cloud from conversation thread prompts
- **WHEN** a user requests a word cloud for prompts in a conversation thread
- **THEN** the system generates a word cloud showing the most frequent words from all prompts in the thread

#### Scenario: Generate word cloud from conversation thread responses
- **WHEN** a user requests a word cloud for responses in a conversation thread
- **THEN** the system generates a word cloud showing the most frequent words from all responses in the thread

#### Scenario: Generate word cloud from multiple conversations
- **WHEN** a user requests a word cloud from multiple conversations
- **THEN** the system generates a word cloud aggregating words from all selected conversations

#### Scenario: Generate word cloud from user's conversations
- **WHEN** a user requests a word cloud from all their conversations
- **THEN** the system generates a word cloud from all prompts or responses across the user's conversations

#### Scenario: Generate word cloud from team's conversations
- **WHEN** a user requests a word cloud from all team conversations
- **THEN** the system generates a word cloud from all prompts or responses across the team's conversations

#### Scenario: Select spiral layout for word cloud
- **WHEN** a user selects spiral layout option for word cloud generation
- **THEN** words are arranged along an expanding spiral path starting from the center
- **AND** collision detection prevents word overlap
- **AND** word size and frequency influence placement along the spiral

#### Scenario: Select force-directed layout for word cloud
- **WHEN** a user selects force-directed layout option
- **THEN** words are arranged using physics simulation with attraction and repulsion forces
- **AND** frequent words cluster together while less frequent words are distributed around them

### Requirement: Word cloud filtering
The system SHALL allow users to filter word cloud content using tag-based query filters.

#### Scenario: Filter word cloud by tags
- **WHEN** a user applies tag filters to word cloud generation
- **THEN** only text from annotations or nodes matching the tag filters is included in the word cloud

#### Scenario: Hierarchical tag filtering for word clouds
- **WHEN** a user applies hierarchical tag filters to word cloud generation
- **THEN** text from nodes matching the tag hierarchy is included
- **AND** users can control the layers or levels of detail displayed

### Requirement: Timeline visualization
The system SHALL provide timeline visualization capabilities for organizing and visualizing sequences of events within a topic (distinct from the Timeline view).

#### Scenario: Create timeline visualization
- **WHEN** a user creates a timeline visualization for a topic
- **THEN** events and temporal data are displayed chronologically
- **AND** the timeline provides a high-level overview of historical development

#### Scenario: Timeline visualization updates dynamically
- **WHEN** temporal data is added to nodes in a timeline visualization
- **THEN** the timeline automatically updates to include new information
- **AND** the timeline remains current and accurate

### Requirement: Hierarchical tag-based filtering
The system SHALL allow users to apply precise and flexible filters using hierarchical tags to control displayed detail levels.

#### Scenario: Filter by hierarchical tags
- **WHEN** a user applies hierarchical tag filters
- **THEN** only data matching the tag hierarchy is displayed
- **AND** users can apply filters at different levels of granularity

#### Scenario: Broad tag filtering
- **WHEN** a user applies a high-level tag filter
- **THEN** data matching the tag and all child tags is displayed
- **AND** a broad view of the data is shown

#### Scenario: Fine-grained tag filtering
- **WHEN** a user applies a specific subtag filter
- **THEN** only data matching the specific subtag is displayed
- **AND** a focused, detailed view of the data is shown

### Requirement: Customizable data filtering
The system SHALL allow users to apply either broad or fine-grained filters to their data depending on research objectives.

#### Scenario: Switch between broad and fine-grained filters
- **WHEN** a user switches between broad and fine-grained tag filters
- **THEN** the view adjusts accordingly
- **AND** users can quickly switch between high-level overviews and in-depth analyses

#### Scenario: Combine multiple filters
- **WHEN** a user combines multiple filter criteria
- **THEN** only data matching all criteria is displayed
- **AND** the combined filters are visually indicated

### Requirement: Data export for analysis
The system SHALL allow users to export filtered and analyzed data for external analysis.

#### Scenario: Export filtered data
- **WHEN** a user exports data after applying filters
- **THEN** only the filtered data is exported
- **AND** the export format preserves the applied filters

#### Scenario: Export analysis results
- **WHEN** a user exports analysis results (e.g., word cloud data, timeline data)
- **THEN** the exported data is formatted appropriately for external analysis tools

### Requirement: Word cloud theming and styling
The system SHALL provide customizable theming for word clouds including predefined color themes, gradients, custom fonts, and background options.

- Predefined color themes SHALL include categorical palettes (e.g., vibrant, pastel, monochrome) and gradient schemes.
- Gradient themes SHALL apply color gradients to words based on frequency or alphabetical order.
- Users SHALL be able to select custom fonts from system fonts or load custom font files.
- Background options SHALL include solid colors, gradients, transparent, and image backgrounds.
- Theme changes SHALL apply immediately to the word cloud visualization.

#### Scenario: Apply gradient theme to word cloud
- **WHEN** a user selects a gradient theme (e.g., blue-to-purple)
- **THEN** word colors are assigned along the gradient spectrum based on frequency
- **AND** higher frequency words use colors from one end of the gradient and lower frequency from the other end

#### Scenario: Change word cloud background
- **WHEN** a user selects a custom background (solid color, gradient, or image)
- **THEN** the word cloud background updates to show the selected style
- **AND** text colors adjust automatically for readability if needed

#### Scenario: Apply custom font to word cloud
- **WHEN** a user selects a custom font for the word cloud
- **THEN** all words in the cloud use the selected font
- **AND** the font is preserved in PNG and SVG exports

### Requirement: Word cloud export to PNG and SVG
The system SHALL allow users to export word clouds as PNG or SVG files with customizable quality settings.

- PNG exports SHALL support DPI scaling (1x-4x) and preserve the current layout, theme, and styling.
- SVG exports SHALL be vector format with embedded fonts or font-family references.
- Exports SHALL include the background if not transparent.
- Export file names SHALL include visualization type, timestamp, and optionally filter context.

#### Scenario: Export word cloud to SVG with custom theme
- **WHEN** a user exports a word cloud with a gradient theme and custom font to SVG
- **THEN** the SVG file contains vector paths for all words
- **AND** the gradient theme and font information are preserved in the SVG
- **AND** fonts are embedded or referenced via font-family

#### Scenario: Export word cloud to high-DPI PNG
- **WHEN** a user exports a word cloud to PNG with 3x DPI setting
- **THEN** the PNG is generated at 3x resolution (216 DPI equivalent)
- **AND** all words, colors, and background are rendered at the higher resolution
- **AND** the file size reflects the increased resolution

### Requirement: Timeline analytics uses shared renderer
The system SHALL reuse the SkiaSharp timeline renderer for analytics timelines to ensure consistent interactions and performance.

#### Scenario: Shared interactions
- **WHEN** an analytics timeline is shown
- **THEN** it supports the same zoom, scroll, selection, and clustering behaviors as the main Timeline view

