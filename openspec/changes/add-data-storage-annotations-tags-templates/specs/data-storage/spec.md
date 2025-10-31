## ADDED Requirements

### Requirement: Persist annotations
The system SHALL persist annotations attached to topic nodes, including text span references, assigned tags, optional numeric/geospatial/temporal values, and free-text comments.

#### Scenario: Save and reload annotation for a node
- **WHEN** a user creates an annotation on a node's response (with start/end offsets)
- **THEN** the annotation (nodeRef, span, tags, values, comment, timestamps) is saved and available after app restart

#### Scenario: Update and delete annotation
- **WHEN** a user edits or deletes an existing annotation
- **THEN** the persisted record is updated or removed accordingly and no longer appears in queries

#### Scenario: Query annotations by tag and node
- **WHEN** annotations are requested for a node with a tag filter (and optional ranges)
- **THEN** only annotations matching the tag(s) and value criteria are returned

#### Scenario: Span integrity on response edit (non-blocking)
- **WHEN** the node's response text changes
- **THEN** existing annotations retain stored offsets; re-anchoring is out of scope for storage and handled by higher layers


### Requirement: Persist tag taxonomy (hierarchical)
The system SHALL persist a hierarchical tag taxonomy usable for annotation tagging, supporting parent/child relationships, display order, and uniqueness among siblings.

#### Scenario: Create and reload taxonomy hierarchy
- **WHEN** a user defines categories and tags as a hierarchy
- **THEN** the structure and ordering persist and load identically after restart

#### Scenario: Update taxonomy nodes
- **WHEN** a category/tag node is renamed, re-ordered, moved, or deleted
- **THEN** the persisted structure reflects the change while maintaining hierarchy integrity

#### Scenario: Referential integrity with annotations
- **WHEN** a taxonomy tag referenced by annotations is deleted
- **THEN** the system enforces a policy (restrict delete or cascade unassign) configurable at a higher layer; storage MUST support foreign key constraints


### Requirement: Persist prompt templates (hierarchical collections)
The system SHALL persist prompt templates organized in a hierarchical collection (folders/categories), with template fields for name/title, body text, and optional placeholders/parameters.

#### Scenario: Save and reload template collections
- **WHEN** a user creates categories and templates
- **THEN** the collection hierarchy and templates persist and reload after restart

#### Scenario: Update and delete templates
- **WHEN** a template or category is renamed, moved, updated, or deleted
- **THEN** the persisted records reflect the change and load accordingly

#### Scenario: Query templates by path or category
- **WHEN** templates are listed under a category or by hierarchical path
- **THEN** only templates within the specified subtree are returned

