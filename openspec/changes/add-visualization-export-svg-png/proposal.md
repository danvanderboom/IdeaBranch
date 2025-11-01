## Why
Current visualizations (Word Cloud, Timeline views, Map) only support basic layouts and limited export options. Users need high-quality visualization exports (PNG/SVG) with customizable layouts, theming, fonts, and backgrounds. Enhanced word cloud layouts (force-directed, spiral), timeline event connections, and professional export capabilities will improve research presentation and analysis.

## What Changes
- Add PNG and SVG export for Word Cloud, Timeline (UI and Analytics), and Map visualizations with high-DPI scaling support (1x-4x)
- Enhance Word Cloud with force-directed and spiral layout options, color themes/gradients, custom fonts and backgrounds
- Add Map export with layer/filter preservation, optional legends, theming, and background customization (solid/gradient/image)
- Add Timeline view export with banded layout preservation, event-to-event connections, and customizable fonts/backgrounds
- Add Analytics Timeline export with connections overlay, theme preservation, and background customization
- **BREAKING**: None (additive features only)

## Impact
- Affected specs: `import-export` (visualization export), `analytics` (word cloud enhancements), `ui` (map and timeline enhancements), `timeline-analytics` (connections and export)
- Affected code: Export service layer, visualization rendering components (SkiaSharp/Canvas), word cloud layout algorithms, timeline connection rendering, map rendering pipeline

