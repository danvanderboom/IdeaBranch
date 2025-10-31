## ADDED Requirements

### Requirement: Graceful error handling for network and model failures
The system MUST handle network and model/API errors without crashes and present actionable messages.

#### Scenario: Network unavailable
- **WHEN** a request is submitted without connectivity
- **THEN** the user sees an error with retry guidance; the app remains responsive

#### Scenario: Model/API error
- **WHEN** the language model returns an error
- **THEN** the user is informed of the failure and can retry or adjust settings

