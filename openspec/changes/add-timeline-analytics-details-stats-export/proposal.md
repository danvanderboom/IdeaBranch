## Why
The current Analytics timeline displays only minimal event titles, limiting users' ability to understand context, navigate to related content, analyze event patterns, and export filtered data for external analysis. Users need expanded event details, links to related nodes and annotations, visual organization by event type bands, statistics to understand trends, and export capabilities.

## What Changes
- Add expandable event cards/drawer showing full event details (title, body, type, actor, source, tags, timestamps, related nodes/annotations)
- Add navigable links from events to related topic nodes and annotations with return-to-context navigation
- Add "Group by type" toggle that organizes events into horizontal bands by event type
- Add statistics module showing per-type counts and time-series trends that update with filters/range
- Add CSV and JSON export of filtered event subsets respecting all active filters

## Impact
- Affected specs: `timeline-analytics` (new requirements)
- Affected code: `IdeaBranch.App/Views/Analytics/TimelinePage.xaml`, `IdeaBranch.App/ViewModels/Analytics/TimelineViewModel.cs`, timeline renderer components, export service layer

