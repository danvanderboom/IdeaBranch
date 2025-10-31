## ADDED Requirements

### Requirement: Product definition centralized in repository docs
The system SHALL maintain the primary product source document at `docs/product/IdeaBranch Product.txt`.

#### Scenario: Product doc discoverability
- **WHEN** a developer opens the repository
- **THEN** the product doc location is documented in `docs/README.md`

### Requirement: Hierarchical topic organization via topic trees
The system SHALL organize research into hierarchical topic trees where each node pairs a prompt with a response.

#### Scenario: List responses create child nodes
- **WHEN** a generated response contains a list
- **THEN** each list item is added as a child node under the current node

