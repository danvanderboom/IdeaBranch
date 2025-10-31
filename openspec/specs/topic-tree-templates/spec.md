# topic-tree-templates Specification

## Purpose
TBD - created by archiving change add-topic-tree-templates. Update Purpose after archive.
## Requirements
### Requirement: Pre-defined topic tree structures
The system SHALL provide pre-defined topic tree templates for getting started with specific kinds of research topics.

#### Scenario: Access topic tree templates
- **WHEN** a user creates a new topic tree
- **THEN** the system offers topic tree templates as an option
- **AND** templates are organized by category or use case

#### Scenario: Template provides initial structure
- **WHEN** a user applies a topic tree template
- **THEN** the template provides an initial structure for the topic
- **AND** teams can get started quickly with a predefined framework

### Requirement: Create templates from existing trees
The system SHALL allow users to create templates from existing topic trees.

#### Scenario: Save tree as template
- **WHEN** a user saves an existing topic tree as a template
- **THEN** the tree structure is saved as a reusable template
- **AND** the template can be named and categorized

#### Scenario: Template includes structure only
- **WHEN** a topic tree is saved as a template
- **THEN** the template includes the tree structure (nodes and hierarchy)
- **AND** user-specific content may be excluded or replaced with placeholders

### Requirement: Apply template to create new topic
The system SHALL allow users to apply templates to create new topic trees.

#### Scenario: Apply template to new topic
- **WHEN** a user applies a template to create a new topic
- **THEN** a new topic tree is created with the template's structure
- **AND** the tree can be populated with topic-specific content

#### Scenario: Template structure preserved
- **WHEN** a template is applied to create a new topic
- **THEN** the template's hierarchical structure is preserved
- **AND** all nodes, categories, and relationships from the template are created

### Requirement: Customize template after application
The system SHALL allow users to customize template structures after they have been applied.

#### Scenario: Modify template structure
- **WHEN** a user applies a template and then modifies the structure
- **THEN** the tree can be customized (add/remove/reorder nodes)
- **AND** changes are preserved in the new topic tree

#### Scenario: Template as starting point
- **WHEN** a template is applied
- **THEN** the template serves as a starting point
- **AND** users can expand, modify, or simplify the structure as needed

### Requirement: Share templates with teams
The system SHALL allow users to share topic tree templates with teams.

#### Scenario: Share template with team
- **WHEN** a user shares a template with a team
- **THEN** team members can access and use the template
- **AND** the template appears in the team's template library

#### Scenario: Team template library
- **WHEN** team members create new topic trees
- **THEN** templates shared with the team are available
- **AND** team templates are organized and accessible

### Requirement: Use case templates
The system SHALL provide templates for specific use cases such as FBI investigations or software solution architecture.

#### Scenario: FBI Suspect Template
- **WHEN** a user applies the FBI Suspect Template
- **THEN** a topic tree structure for organizing suspect information is created
- **AND** nodes include Personal Information, Activities, and Connections

#### Scenario: Use Case Template
- **WHEN** a user applies the Use Case Template for software solution architecture
- **THEN** a topic tree structure for documenting use cases is created
- **AND** nodes include Overview, Actors, Preconditions, Steps, Postconditions, and Non-Functional Requirements

