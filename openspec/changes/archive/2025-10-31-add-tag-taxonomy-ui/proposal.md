## Why
The data-storage spec requires persistence for hierarchical tag taxonomies, but the UI layer for creating, editing, viewing, and managing tag taxonomies is not specified. Users need to be able to organize tags hierarchically, reorder tags, and leverage AI assistance for taxonomy creation.

## What Changes
- Add new `tag-taxonomy-ui` capability spec
- Specify UI requirements for viewing hierarchical tag taxonomy
- Specify requirements for creating, editing, and deleting categories and tags
- Specify requirements for reordering tags within siblings
- Specify requirements for moving tags between categories
- Specify requirements for import/export taxonomy
- Specify requirements for AI-assisted taxonomy generation

## Impact
- Affected specs: tag-taxonomy-ui (new capability)
- Affected code: UI components for tag taxonomy management (not yet implemented - this is spec-only)

