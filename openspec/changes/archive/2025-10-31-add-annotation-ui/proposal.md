## Why
The data-storage spec requires persistence for annotations, but the UI layer for creating, editing, viewing, and managing annotations is not specified. Users need to be able to select text spans, attach tags, add comments, and filter annotations through an intuitive interface.

## What Changes
- Add new `annotations-ui` capability spec
- Specify UI requirements for text span selection and highlighting
- Specify requirements for creating, editing, and deleting annotations
- Specify requirements for attaching tags from taxonomy
- Specify requirements for adding numeric/geospatial/temporal values
- Specify requirements for comment management and visibility toggle
- Specify requirements for filtering annotations by tag

## Impact
- Affected specs: annotations-ui (new capability)
- Affected code: UI components for annotations (not yet implemented - this is spec-only)

