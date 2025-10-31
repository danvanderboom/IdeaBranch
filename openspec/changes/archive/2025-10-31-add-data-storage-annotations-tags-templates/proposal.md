## Why
The data-storage spec currently only requires persistence for topic trees and version history. According to the product documentation, the system also needs to persist annotations (with tags and comments), tag taxonomies (hierarchical), and prompt templates (hierarchical collections). Adding these storage requirements to the spec ensures they are properly specified before implementation.

## What Changes
- Add spec requirements for persisting annotations attached to topic nodes
- Add spec requirements for persisting hierarchical tag taxonomies
- Add spec requirements for persisting hierarchical prompt template collections
- Include scenarios covering save, load, update, delete, and query operations for each

## Impact
- Affected specs: data-storage (ADDED requirements for annotations, tag taxonomies, prompt templates)
- Affected code: None (spec-only change - implementation will follow in future changes)

