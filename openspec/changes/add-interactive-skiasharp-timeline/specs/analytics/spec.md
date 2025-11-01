## ADDED Requirements
### Requirement: Timeline analytics uses shared renderer
The system SHOULD reuse the SkiaSharp timeline renderer for analytics timelines to ensure consistent interactions and performance.

#### Scenario: Shared interactions
- **WHEN** an analytics timeline is shown
- **THEN** it supports the same zoom, scroll, selection, and clustering behaviors as the main Timeline view

