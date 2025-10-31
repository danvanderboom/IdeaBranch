## Why
Agents benefit from richer local tooling over in-memory hierarchical data to reduce round-trips, improve determinism, and enable structure-aware editing and navigation without introducing server-side components.

## What Changes
- Add extended agent tool functions (read/navigation, selection/search, structural editing, view control, tagging/bookmarks, validation, diffing, import/export) applicable to local in-memory trees
- Exclude server-side functionality (aggregation, external connectivity), transactional batching, concurrency locks, and event streams

## Impact
- Affected specs: `agent-interface` (new ADDED requirements for extended tools)
- Code: future change will implement these tools as in-process functions layered on existing model/view/controller

