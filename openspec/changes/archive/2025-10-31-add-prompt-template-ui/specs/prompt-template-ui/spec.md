## ADDED Requirements

### Requirement: View hierarchical template collection
The system SHALL allow users to view the hierarchical prompt template collection with categories and templates organized in parent-child relationships.

#### Scenario: Display template tree
- **WHEN** a user opens the prompt template view
- **THEN** the hierarchical template collection is displayed
- **AND** categories and templates are organized in a tree structure with expand/collapse

#### Scenario: Navigate template hierarchy
- **WHEN** a user expands or collapses nodes in the template tree
- **THEN** child categories and templates are shown or hidden accordingly
- **AND** the hierarchy structure is visually clear

### Requirement: Create and edit templates
The system SHALL allow users to create new template categories and templates, and edit existing templates.

#### Scenario: Create new category
- **WHEN** a user creates a new category in the template collection
- **THEN** a category node is added at the selected location in the hierarchy
- **AND** the user can name the category
- **AND** the category can contain child categories or templates

#### Scenario: Create new template
- **WHEN** a user creates a new template in a category
- **THEN** a template node is added as a child of the selected category
- **AND** the user can name the template and add template body text
- **AND** placeholders can be added to the template body (e.g., {keyword}, {phrase})

#### Scenario: Edit template
- **WHEN** a user edits an existing template
- **THEN** the edit dialog/form is presented with current name and body
- **AND** changes are saved when confirmed

#### Scenario: Delete template
- **WHEN** a user deletes a template or category
- **THEN** the node is removed from the template collection
- **AND** if the node has children, the system handles deletion according to policy

### Requirement: Template body with placeholders
The system SHALL allow users to create template bodies with placeholders for user-specified keywords or phrases.

#### Scenario: Add placeholder to template
- **WHEN** a user creates or edits a template body
- **THEN** the user can add placeholders using syntax like {keyword} or {phrase}
- **AND** placeholders are visually indicated in the template

#### Scenario: Apply template with placeholder substitution
- **WHEN** a user applies a template to a topic node
- **THEN** placeholders are replaced with user-specified values
- **AND** the resulting prompt is inserted into the topic node's prompt field

### Requirement: Apply template to topic node
The system SHALL allow users to apply prompt templates to topic nodes.

#### Scenario: Apply template from view
- **WHEN** a user selects a template and chooses to apply it
- **THEN** the template is applied to the currently selected topic node
- **AND** the template body (with placeholder substitution) replaces or augments the node's prompt

#### Scenario: Apply template from topic node
- **WHEN** a user is editing a topic node's prompt
- **THEN** the user can access and apply templates
- **AND** template recommendations can be shown (if enabled in settings)

### Requirement: Search templates by path
The system SHALL allow users to find templates by their hierarchical path in the template collection.

#### Scenario: Search by path
- **WHEN** a user searches for a template by path (e.g., "Information Retrieval/Definitions and explanations")
- **THEN** templates matching the path are returned
- **AND** the hierarchical path is displayed in results

#### Scenario: Browse templates by category
- **WHEN** a user browses templates by category
- **THEN** only templates within the selected category and its subcategories are shown
- **AND** the category hierarchy is navigable

### Requirement: AI-assisted template generation
The system SHALL allow users to generate prompt template categories and templates automatically using AI.

#### Scenario: Generate template categories
- **WHEN** a user provides a description and requests AI generation of template categories
- **THEN** the system generates a hierarchical category structure
- **AND** the generated structure is presented for review and editing

#### Scenario: Generate templates
- **WHEN** a user requests AI generation of templates for a category
- **THEN** the system generates prompt templates for the category
- **AND** multiple attempts can be made until desired templates are achieved

#### Scenario: Manual override
- **WHEN** a user chooses to create or edit templates manually
- **THEN** the user can create, edit, and organize templates without AI assistance
- **AND** manual and AI-generated templates can be combined

