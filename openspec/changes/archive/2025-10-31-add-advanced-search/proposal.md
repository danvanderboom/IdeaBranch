## Why
The product documentation describes comprehensive search and filtering capabilities for topic trees, including tag-based filtering, tag expressions, text search, and time range queries. These advanced search features are not currently specified in any capability spec.

## What Changes
- Add new `search` capability spec
- Specify requirements for searching by content type (nodes, annotations, tags, templates)
- Specify requirements for tag-based filtering (single/multiple tags, exclusion)
- Specify requirements for tag expressions (AND/OR/BUT-NOT-IF)
- Specify requirements for tag weight range queries
- Specify requirements for text search (exact/similar)
- Specify requirements for edit time range filtering
- Specify requirements for historical time range filtering

## Impact
- Affected specs: search (new capability)
- Affected code: Search functionality (not yet implemented - this is spec-only)

