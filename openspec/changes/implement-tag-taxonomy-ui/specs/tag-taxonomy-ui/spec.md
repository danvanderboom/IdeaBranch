## MODIFIED Requirements

### Requirement: View hierarchical tag taxonomy
The system SHALL allow users to view the hierarchical tag taxonomy tree with categories and tags organized in parent-child relationships. **Implementation in progress.**

#### Scenario: Display taxonomy tree
- **WHEN** a user opens the tag taxonomy view
- **THEN** the hierarchical taxonomy tree is displayed
- **AND** categories and tags are organized in a tree structure with expand/collapse

#### Scenario: Navigate taxonomy hierarchy
- **WHEN** a user expands or collapses nodes in the taxonomy tree
- **THEN** child categories and tags are shown or hidden accordingly
- **AND** the hierarchy structure is visually clear

### Requirement: Create and edit taxonomy nodes
The system SHALL allow users to create new categories and tags in the taxonomy, and edit existing taxonomy nodes. **Implementation in progress.**

#### Scenario: Create new category
- **WHEN** a user creates a new category in the taxonomy
- **THEN** a category node is added at the selected location in the hierarchy
- **AND** the user can name the category
- **AND** the category can contain child categories or tags

#### Scenario: Create new tag
- **WHEN** a user creates a new tag in a category
- **THEN** a tag node is added as a child of the selected category
- **AND** the user can name the tag
- **AND** the tag is available for annotation use

#### Scenario: Edit taxonomy node
- **WHEN** a user edits an existing category or tag
- **THEN** the edit dialog/form is presented with current name
- **AND** changes are saved when confirmed

#### Scenario: Delete taxonomy node
- **WHEN** a user deletes a category or tag
- **THEN** the node is removed from the taxonomy
- **AND** if the node has children, the system handles deletion according to policy (restrict or cascade)
- **AND** if the node is referenced by annotations, the system enforces deletion policy

### Requirement: Reorder tags within siblings
The system SHALL allow users to reorder tags and categories within their sibling group. **Implementation in progress.**

#### Scenario: Reorder using up/down controls
- **WHEN** a user uses up/down controls to reorder a tag
- **THEN** the tag moves within its sibling group
- **AND** the order is updated and persisted

### Requirement: Move tags between categories
The system SHALL allow users to move tags and categories between different parent categories. **Implementation in progress.**

#### Scenario: Move tag to different category
- **WHEN** a user moves a tag from one category to another
- **THEN** the tag's parent relationship is updated
- **AND** the tag appears in its new parent category
- **AND** the move is persisted

#### Scenario: Move category to different parent
- **WHEN** a user moves a category to a different parent category
- **THEN** the category's parent relationship is updated
- **AND** all child nodes move with the category
- **AND** the move is persisted

### Requirement: Import and export taxonomy
The system SHALL allow users to import tag taxonomies from external sources and export taxonomies for sharing or backup. **Implementation in progress.**

#### Scenario: Export taxonomy
- **WHEN** a user exports the tag taxonomy
- **THEN** the taxonomy structure is exported to a file (e.g., JSON, XML)
- **AND** all categories, tags, and hierarchy relationships are included

#### Scenario: Import taxonomy
- **WHEN** a user imports a tag taxonomy from a file
- **THEN** the taxonomy structure is loaded and validated
- **AND** imported categories and tags are added to the taxonomy
- **AND** hierarchy relationships are preserved

### Requirement: AI-assisted taxonomy generation
The system SHALL allow users to generate tag taxonomies automatically using AI based on a description of the taxonomy's purpose. **Implementation in progress.**

#### Scenario: Generate taxonomy from description
- **WHEN** a user provides a description of the taxonomy's purpose and requests AI generation
- **THEN** the system generates a hierarchical tag taxonomy based on the description
- **AND** the generated taxonomy is presented for review and editing

