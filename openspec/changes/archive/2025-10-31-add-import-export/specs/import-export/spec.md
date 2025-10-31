## ADDED Requirements

### Requirement: Export topic trees
The system SHALL allow users to export hierarchical topic trees to files in JSON, XML, and Markdown formats.

#### Scenario: Export to JSON
- **WHEN** a user exports a topic tree to JSON
- **THEN** the topic tree structure is exported to a JSON file
- **AND** all nodes, prompts, responses, and hierarchy relationships are preserved

#### Scenario: Export to XML
- **WHEN** a user exports a topic tree to XML
- **THEN** the topic tree structure is exported to an XML file
- **AND** all nodes, prompts, responses, and hierarchy relationships are preserved

#### Scenario: Export to Markdown
- **WHEN** a user exports a topic tree to Markdown
- **THEN** the topic tree structure is exported to a Markdown file
- **AND** the hierarchical structure is represented using Markdown heading levels
- **AND** prompts and responses are formatted appropriately

### Requirement: Import topic trees
The system SHALL allow users to import hierarchical topic trees from files in JSON, XML, and Markdown formats.

#### Scenario: Import from JSON
- **WHEN** a user imports a topic tree from a JSON file
- **THEN** the topic tree structure is loaded and validated
- **AND** all nodes, prompts, responses, and hierarchy relationships are preserved

#### Scenario: Import from XML
- **WHEN** a user imports a topic tree from an XML file
- **THEN** the topic tree structure is loaded and validated
- **AND** all nodes, prompts, responses, and hierarchy relationships are preserved

#### Scenario: Import from Markdown
- **WHEN** a user imports a topic tree from a Markdown file
- **THEN** the topic tree structure is parsed from Markdown heading levels
- **AND** prompts and responses are extracted from the Markdown content
- **AND** the hierarchy is reconstructed from heading structure

### Requirement: Format validation
The system SHALL validate imported topic tree files for format correctness before importing.

#### Scenario: Validate JSON format
- **WHEN** a user attempts to import a JSON file
- **THEN** the system validates the JSON structure
- **AND** if invalid, an error is reported and import is prevented

#### Scenario: Validate XML format
- **WHEN** a user attempts to import an XML file
- **THEN** the system validates the XML structure
- **AND** if invalid, an error is reported and import is prevented

#### Scenario: Validate Markdown format
- **WHEN** a user attempts to import a Markdown file
- **THEN** the system validates the Markdown structure can be parsed
- **AND** if invalid, an error is reported and import is prevented

### Requirement: Document import
The system SHALL allow users to import research documents and papers from external sources (e.g., Arxiv.org).

#### Scenario: Import document from Arxiv.org
- **WHEN** a user imports a document from Arxiv.org
- **THEN** the system retrieves the document content or metadata
- **AND** the document is associated with the selected topic node

#### Scenario: Document as single node
- **WHEN** a user imports a document and chooses to represent it as a single reference document
- **THEN** the topic node represents the entire document
- **AND** document content or metadata is stored with the node

#### Scenario: Document as one of many
- **WHEN** a user imports a document into a node that already contains other documents
- **THEN** the document is added to the list of documents in the node
- **AND** multiple documents can coexist in the same node

