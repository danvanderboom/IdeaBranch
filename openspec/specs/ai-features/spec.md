# ai-features Specification

## Purpose
TBD - created by archiving change add-ai-features. Update Purpose after archive.
## Requirements
### Requirement: LLM prompt submission from topic nodes
The system SHALL allow users to submit prompts to language models from any topic node, with the generated response appearing within the node.

#### Scenario: Submit prompt from topic node
- **WHEN** a user writes a prompt in a topic node and submits it
- **THEN** the prompt is sent to the configured language model
- **AND** the generated response appears within the topic node

#### Scenario: Configure response parameters
- **WHEN** a user submits a prompt
- **THEN** the user can optionally configure response length and other parameters
- **AND** the configured parameters are applied to the LLM request

#### Scenario: List items create child nodes
- **WHEN** a generated response contains a list
- **THEN** each list item is automatically added as a child node under the current node
- **AND** all list items are sibling nodes with the same parent

#### Scenario: Multiple lists create intermediate nodes
- **WHEN** a generated response contains multiple lists
- **THEN** intermediate nodes are utilized to maintain a clear structure
- **AND** each list is organized as a subtree

### Requirement: Automated title generation
The system SHALL allow users to request automated generation of titles that succinctly capture the essence of a prompt and its response.

#### Scenario: Generate title from prompt and response
- **WHEN** a user requests title generation for a topic node
- **THEN** the system generates a title based on the prompt and response
- **AND** the title is proposed for user acceptance or editing

### Requirement: Prompt template recommendations
The system SHALL allow users to request recommendations for prompt templates, and optionally auto-recommend templates when editing prompts.

#### Scenario: Request template recommendations
- **WHEN** a user requests template recommendations
- **THEN** the system suggests relevant prompt templates based on context
- **AND** the user can apply a recommended template

#### Scenario: Auto-recommend templates setting
- **WHEN** a user enables auto-recommend templates in settings
- **THEN** template recommendations are shown when editing the prompt field in a topic node
- **AND** recommendations are contextually relevant

### Requirement: Response editing and tracking
The system SHALL allow users to edit LLM-generated responses, with the system capturing edit metadata.

#### Scenario: Edit response with tracking
- **WHEN** a user edits a generated response
- **THEN** the system captures the user who made the edit
- **AND** the system captures the time and date of the edit
- **AND** the original response text is preserved for future reference

### Requirement: Automated topic tree creation
The system SHALL allow users to automatically create initial topic trees using language models based on a description.

#### Scenario: Generate topic tree from description
- **WHEN** a user provides a description of a topic and requests automatic tree creation
- **THEN** the system generates an initial topic tree using language models
- **AND** the tree structure is based on the description and specified aspects/viewpoints
- **AND** the generated tree can be customized and expanded by users

### Requirement: AI-assisted tag taxonomy generation
The system SHALL allow users to automatically generate tag taxonomies using AI based on a description of the taxonomy's purpose.

#### Scenario: Generate taxonomy from description
- **WHEN** a user provides a description of the taxonomy's purpose and requests AI generation
- **THEN** the system generates a hierarchical tag taxonomy based on the description
- **AND** multiple attempts can be made until the desired taxonomy is achieved

#### Scenario: Manual override for taxonomy
- **WHEN** a user chooses to create taxonomy manually
- **THEN** the user can create, edit, and organize taxonomy nodes without AI assistance
- **AND** manual and AI-generated taxonomies can be combined

### Requirement: AI-assisted prompt template generation
The system SHALL allow users to automatically generate prompt template categories and templates using AI.

#### Scenario: Generate template categories
- **WHEN** a user provides a description and requests AI generation of template categories
- **THEN** the system generates a hierarchical category structure
- **AND** the generated structure is presented for review and editing

#### Scenario: Generate templates for category
- **WHEN** a user requests AI generation of templates for a category
- **THEN** the system generates prompt templates for the category
- **AND** templates can be generated for some or all existing categories
- **AND** multiple attempts can be made until desired templates are achieved

#### Scenario: Manual override for templates
- **WHEN** a user chooses to create templates manually
- **THEN** the user can create, edit, and organize templates without AI assistance
- **AND** manual and AI-generated templates can be combined

### Requirement: AI safety
The system SHALL evaluate generated language requests and responses for appropriateness using NLP techniques.

#### Scenario: Detect offensive language in request
- **WHEN** a user submits a prompt containing offensive or abusive language
- **THEN** the system detects the inappropriate content
- **AND** the application rejects the request
- **AND** the user is informed of the rejection

#### Scenario: Detect offensive language in response
- **WHEN** a language model generates a response containing offensive or abusive language
- **THEN** the system detects the inappropriate content
- **AND** the application rejects the response
- **AND** the user is informed of the rejection

#### Scenario: Protect users from negative consequences
- **WHEN** potentially negative content is generated
- **THEN** the system evaluates the content for appropriateness
- **AND** users and teams are protected from potential negative consequences

