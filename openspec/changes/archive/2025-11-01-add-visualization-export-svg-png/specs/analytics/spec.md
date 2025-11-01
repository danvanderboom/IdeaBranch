## MODIFIED Requirements

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

## ADDED Requirements

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

