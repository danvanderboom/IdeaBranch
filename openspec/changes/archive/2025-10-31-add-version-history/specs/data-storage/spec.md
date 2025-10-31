## MODIFIED Requirements

### Requirement: Version history
The system SHALL record version history of edits to enable viewing past states and auditing changes.

#### Scenario: Save creates version history entry
- **WHEN** a user edits and saves a topic node (title, prompt, or response)
- **THEN** a new version history entry is created with the previous state and timestamp

#### Scenario: View version history for a node
- **WHEN** the user opens history for a topic node
- **THEN** prior edits (with author and timestamp) are listed in reverse chronological order

#### Scenario: Version history persists across restarts
- **WHEN** the app restarts after creating version history
- **THEN** the version history entries are still available and queryable

