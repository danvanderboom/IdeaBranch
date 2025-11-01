## ADDED Requirements

### Requirement: Export Visualizations as PNG and SVG
The system SHALL allow users to export visualizations (Word Cloud, Timeline views, Map) as PNG or SVG files with high-DPI scaling support.

- PNG exports SHALL support DPI scaling from 1x to 4x (e.g., 1x=72 DPI, 2x=144 DPI, 3x=216 DPI, 4x=288 DPI).
- SVG exports SHALL be vector format preserving all styles, fonts, and colors from the visualization.
- Exports SHALL respect all active filters, viewport settings, and visible layers.
- Export processes SHALL be non-blocking with progress indication and file naming prompts.
- File names SHALL be automatically generated based on visualization type, timestamp, and filters if applicable.

#### Scenario: Export Word Cloud to PNG with high DPI
- **WHEN** a user selects Export → PNG and sets DPI to 2x
- **THEN** a PNG file is generated at 2x resolution reflecting the current word cloud viewport and filters
- **AND** the file is saved with an appropriate name and progress is shown during export

#### Scenario: Export Timeline to SVG
- **WHEN** a user selects Export → SVG from the Timeline view
- **THEN** a vector SVG file is generated preserving all timeline styles, fonts, colors, and banded layout
- **AND** the SVG includes embedded or referenced fonts and maintains the exact visual appearance

#### Scenario: Export Map with filters applied
- **WHEN** a user exports the Map view with active tag and layer filters
- **THEN** the exported PNG/SVG contains only the filtered data and visible layers
- **AND** the export respects the current map zoom level and viewport

#### Scenario: Export Analytics Timeline with connections
- **WHEN** a user exports the Analytics Timeline with event connections visible
- **THEN** the exported file includes the connection lines between events
- **AND** the banded layout, legend, and statistics are preserved if included in the viewport

