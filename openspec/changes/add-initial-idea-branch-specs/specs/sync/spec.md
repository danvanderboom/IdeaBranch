## ADDED Requirements

### Requirement: Offline edits with background sync
The system SHALL allow users to edit topics and annotations offline and synchronize changes when connectivity is restored.

#### Scenario: Edit offline then sync on reconnect
- **WHEN** the user edits a topic while offline
- **THEN** the change is saved locally and is synchronized automatically when the app reconnects

### Requirement: Edit conflict handling baseline
The system SHALL record concurrent edits in version history and apply last-writer-wins to the current state.

#### Scenario: Concurrent edit produces deterministic result
- **WHEN** two users edit the same node concurrently
- **THEN** the latest timestamped edit is applied to current state and prior edits remain visible in history

