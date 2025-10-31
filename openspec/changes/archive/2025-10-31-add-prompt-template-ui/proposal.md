## Why
The data-storage spec requires persistence for hierarchical prompt template collections, but the UI layer for creating, editing, viewing, and managing prompt templates is not specified. Users need to be able to organize templates hierarchically, apply templates to topic nodes, and leverage AI assistance for template generation.

## What Changes
- Add new `prompt-template-ui` capability spec
- Specify UI requirements for viewing hierarchical template collection
- Specify requirements for creating, editing, and deleting templates and categories
- Specify requirements for template body with placeholders
- Specify requirements for applying template to topic node
- Specify requirements for searching templates by path
- Specify requirements for AI-assisted template generation

## Impact
- Affected specs: prompt-template-ui (new capability)
- Affected code: UI components for prompt template management (not yet implemented - this is spec-only)

