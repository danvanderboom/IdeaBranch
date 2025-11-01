# Release Notes

## Visualization Export Feature

### Overview

This release introduces comprehensive export capabilities for all visualization types (Word Cloud, Timeline, and Map) with support for PNG and SVG formats, customizable theming, and high-DPI scaling.

### Features

#### Word Cloud Export

- **Export Formats**: PNG and SVG
- **Layout Options**: Random, Spiral, and Force-Directed layouts
- **Theming**:
  - Color palettes (vibrant, pastel, etc.)
  - Custom font families
  - Gradient color mapping for words based on frequency
  - Solid color or transparent backgrounds
- **DPI Scaling**: 1x–4x for PNG exports (1x = 72 DPI, 4x = 288 DPI)
- **Export Location**: Files are saved to app data directory and opened via system share dialog

#### Timeline Export

- **Export Formats**: PNG and SVG
- **Optional Features**:
  - Event-to-event connections (automatically generated for events with same NodeId)
  - Statistics panel inclusion
- **Theming**:
  - Color palettes
  - Custom font families
  - Solid color or transparent backgrounds
- **DPI Scaling**: 1x–4x for PNG exports
- **Export Location**: Files are saved to app data directory and opened via system share dialog
- **Progress Indication**: Loading spinner and "Exporting..." label during export operations

#### Map Export

- **Export Formats**: PNG and SVG
- **Optional Features**:
  - Base map tiles inclusion
  - Legend inclusion
- **Theming**:
  - Color palettes
  - Custom font families
  - Solid color or transparent backgrounds
- **DPI Scaling**: 1x–4x for PNG exports
- **Export Location**: Files are saved to app data directory and opened via system share dialog
- **Progress Indication**: Loading spinner and "Exporting..." label during export operations

### Configuration

All export settings are configurable in **Settings > Export**:

- **DPI Scale**: 1x–4x (default: 1x)
- **Background Color**: Hex color code (default: #FFFFFF)
- **Transparent Background**: Boolean toggle (default: false)
- **Font Family**: Custom font family name (optional)
- **Color Palette**: Predefined palette selection (default: vibrant)
- **Word Cloud Layout**: Random, Spiral, or Force-Directed (default: Random)
- **Include Legend**: Boolean toggle for map and timeline exports (default: true)
- **Timeline Connections**: Boolean toggle for timeline event connections (default: false)
- **Timeline Statistics**: Boolean toggle for timeline statistics panel (default: false)
- **Map Tiles**: Boolean toggle for base map tiles (default: false)

### Usage

See [Analytics Documentation](development/analytics.md#how-to-export-visualizations) for detailed step-by-step instructions on exporting each visualization type.

### Technical Details

- **Export Service**: `AnalyticsExportService` handles all export operations
- **Rendering**: SkiaSharp for PNG rendering, custom SVG writer for vector output
- **Settings Integration**: `SettingsService` manages all export preferences
- **File Sharing**: Uses MAUI `Share` API for cross-platform file sharing

### Breaking Changes

None. This is a new feature addition with no breaking changes to existing functionality.

### Future Enhancements

- Additional export formats (Excel, PDF)
- Custom export templates
- Batch export operations
- Scheduled exports

