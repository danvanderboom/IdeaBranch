## ADDED Requirements

### Requirement: Text span selection and highlighting
The system SHALL allow users to select text spans in topic node responses and visually highlight selected spans for annotation.

#### Scenario: Select text span for annotation
- **WHEN** a user selects a text span in a topic node's response
- **THEN** the selected span is visually highlighted
- **AND** an annotation creation interface is presented

#### Scenario: Highlight existing annotations
- **WHEN** a topic node's response contains existing annotations
- **THEN** annotated text spans are visually indicated (e.g., with background color or underline)
- **AND** users can interact with highlighted spans to view/edit annotations

### Requirement: Create and edit annotations
The system SHALL allow users to create new annotations on selected text spans and edit existing annotations.

#### Scenario: Create new annotation
- **WHEN** a user selects a text span and chooses to create an annotation
- **THEN** an annotation dialog/form is presented
- **AND** the user can add tags, comments, and optional values (numeric/geospatial/temporal)
- **AND** the annotation is saved and displayed on the text span

#### Scenario: Edit existing annotation
- **WHEN** a user clicks on an existing annotation highlight
- **THEN** the annotation edit dialog/form is presented with current values
- **AND** the user can modify tags, comments, and values
- **AND** changes are saved when confirmed

#### Scenario: Delete annotation
- **WHEN** a user deletes an annotation
- **THEN** the annotation is removed
- **AND** the text span highlight is removed

### Requirement: Attach tags from taxonomy
The system SHALL allow users to attach one or more tags from the hierarchical tag taxonomy to annotations.

#### Scenario: Select tag from taxonomy
- **WHEN** a user creates or edits an annotation
- **THEN** the user can browse and select tags from the hierarchical tag taxonomy
- **AND** selected tags are attached to the annotation

#### Scenario: Filter tags by taxonomy
- **WHEN** a user is selecting tags for an annotation
- **THEN** only tags from the configured taxonomy are available (if taxonomy restriction is enabled)
- **AND** tags are organized hierarchically in the selection interface

### Requirement: Add annotation values
The system SHALL allow users to attach optional numeric, geospatial, or temporal values to annotations.

#### Scenario: Add numeric value
- **WHEN** a user creates or edits an annotation
- **THEN** the user can optionally attach a numeric value (e.g., weight, score)
- **AND** the value is stored with the annotation and available for filtering

#### Scenario: Add geospatial value
- **WHEN** a user creates or edits an annotation
- **THEN** the user can optionally attach a geospatial value (location or region)
- **AND** the annotation is displayed on the Map view when geospatial data is present

#### Scenario: Add temporal value
- **WHEN** a user creates or edits an annotation
- **THEN** the user can optionally attach a temporal value (point in time, date, or timespan)
- **AND** the annotation is displayed on the Timeline view when temporal data is present

### Requirement: Comment management
The system SHALL allow users to add, edit, and view comments on annotations.

#### Scenario: Add comment to annotation
- **WHEN** a user creates or edits an annotation
- **THEN** the user can optionally add a free-text comment
- **AND** the comment is associated with the annotation

#### Scenario: View annotation comments
- **WHEN** an annotation has a comment
- **THEN** the comment is visible by default
- **AND** comments are displayed in a way that indicates their association with the annotation

#### Scenario: Hide/show comments toggle
- **WHEN** a user toggles the "hide comments" setting
- **THEN** comments are hidden from the UI while annotations remain visible
- **AND** the setting persists across sessions

### Requirement: Filter annotations
The system SHALL allow users to filter displayed annotations by tags.

#### Scenario: Filter by single tag
- **WHEN** a user applies a tag filter
- **THEN** only annotations with the selected tag are displayed
- **AND** other annotations are hidden from view

#### Scenario: Filter by multiple tags
- **WHEN** a user applies multiple tag filters
- **THEN** only annotations matching all selected tags are displayed
- **AND** the filter combination is visually indicated

#### Scenario: Clear filters
- **WHEN** a user clears annotation filters
- **THEN** all annotations are displayed again

