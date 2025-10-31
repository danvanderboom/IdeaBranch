# product Specification

## Purpose
TBD - created by archiving change add-initial-idea-branch-specs. Update Purpose after archive.
## Requirements
### Requirement: Product definition centralized in repository docs
The system SHALL maintain the primary product source document at `docs/product/IdeaBranch Product.txt`.

#### Scenario: Product doc discoverability
- **WHEN** a developer opens the repository
- **THEN** the product doc location is documented in `docs/README.md`

### Requirement: Hierarchical topic organization via topic trees
The system SHALL organize research into hierarchical topic trees where each node pairs a prompt with a response. Users can interact with and shape the tree structure by adding new nodes, deleting nodes, and moving nodes up or down among siblings or to different parent nodes. Nodes can be moved to different parent nodes or promoted to their own root topics.

#### Scenario: List responses create child nodes
- **WHEN** a generated response contains a list
- **THEN** each list item is added as a child node under the current node
- **AND** all list items are sibling nodes with the same parent

#### Scenario: Multiple lists create intermediate nodes
- **WHEN** a generated response contains multiple lists
- **THEN** intermediate nodes are utilized to maintain a clear structure
- **AND** each list is organized as a subtree with appropriate parent-child relationships

#### Scenario: Add new node
- **WHEN** a user adds a new node to the topic tree
- **THEN** the node is added at the selected location in the hierarchy
- **AND** the node can contain a prompt and response pair

#### Scenario: Delete node
- **WHEN** a user deletes a node from the topic tree
- **THEN** the node is removed from the tree
- **AND** if the node has children, child nodes are handled according to deletion policy (delete cascade or orphan handling)

#### Scenario: Move node among siblings
- **WHEN** a user moves a node up or down among its siblings
- **THEN** the node's order within its sibling group is updated
- **AND** the new order is persisted

#### Scenario: Move node to different parent
- **WHEN** a user moves a node to a different parent node
- **THEN** the node's parent relationship is updated
- **AND** the node appears as a child of the new parent
- **AND** the move is persisted

#### Scenario: Promote node to root topic
- **WHEN** a user promotes a node to its own root topic
- **THEN** the node becomes a root-level topic
- **AND** the node and its descendants are preserved
- **AND** the promotion is persisted

### Requirement: Document import
The system SHALL allow users to import research documents and papers (e.g., from Arxiv.org) into topic nodes.

#### Scenario: Import document into node
- **WHEN** a user imports a document into a topic node
- **THEN** the document content or reference is added to the node
- **AND** the document can be one of many listed in a node or the node can represent a single reference document

#### Scenario: Import from Arxiv.org
- **WHEN** a user imports a document from Arxiv.org
- **THEN** the system retrieves and imports the document content or metadata
- **AND** the document is associated with the selected topic node

### Requirement: Enhanced context for LLM requests
The system SHALL include hierarchical context (path from node to root) in language model requests to provide relevant context for improved understanding and exploration.

#### Scenario: Include path to root in LLM request
- **WHEN** a user submits an LLM request from a topic node
- **THEN** the system includes the path of content traced from the node's parent to the root
- **AND** if the path exceeds API request limits, the system includes as much context as possible toward the root
- **AND** the most relevant context for the inquiry is preserved and leveraged

#### Scenario: Context preserves hierarchy
- **WHEN** hierarchical context is included in an LLM request
- **THEN** the hierarchical structure and relationships are preserved in the context
- **AND** the context reflects the topic tree's organization and flow

