# import-export Specification

## Purpose
TBD - created by archiving change add-import-export. Update Purpose after archive.
## Requirements
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

### Requirement: Export Visualizations as PNG and SVG
The system SHALL allow users to export visualizations (Word Cloud, Timeline views, Map) as PNG or SVG files with high-DPI scaling support.

- PNG exports SHALL support DPI scaling from 1x to 4x (e.g., 1x=72 DPI, 2x=144 DPI, 3x=216 DPI, 4x=288 DPI).
- SVG exports SHALL be vector format preserving all styles, fonts, and colors from the visualization.
- Exports SHALL respect all active filters, viewport settings, and visible layers.
- Export processes SHALL be non-blocking with progress indication and file naming prompts.
- File names SHALL be automatically generated based on visualization type, timestamp, and filters if applicable.

#### Scenario: Export Word Cloud to PNG with high DPI
- **WHEN** a user selects Export → PNG and sets DPI to 2x
- **THEN** a PNG file is generated at 2x resolution reflecting the current word cloud viewport and filters
- **AND** the file is saved with an appropriate name and progress is shown during export

#### Scenario: Export Timeline to SVG
- **WHEN** a user selects Export → SVG from the Timeline view
- **THEN** a vector SVG file is generated preserving all timeline styles, fonts, colors, and banded layout
- **AND** the SVG includes embedded or referenced fonts and maintains the exact visual appearance

#### Scenario: Export Map with filters applied
- **WHEN** a user exports the Map view with active tag and layer filters
- **THEN** the exported PNG/SVG contains only the filtered data and visible layers
- **AND** the export respects the current map zoom level and viewport

#### Scenario: Export Analytics Timeline with connections
- **WHEN** a user exports the Analytics Timeline with event connections visible
- **THEN** the exported file includes the connection lines between events
- **AND** the banded layout, legend, and statistics are preserved if included in the viewport

