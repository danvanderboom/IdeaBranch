## ADDED Requirements

### Requirement: Usage and crash telemetry with privacy controls
The system SHALL collect anonymized usage and crash telemetry to improve quality, with clear opt-in/out controls.

#### Scenario: Crash telemetry recorded
- **WHEN** the app crashes or encounters an unhandled exception
- **THEN** a crash report is recorded and queued for upload respecting user consent

### Requirement: Feature usage events for analytics
The system SHALL emit telemetry events for core features (navigation, topic edits, search) to support analytics.

#### Scenario: Navigation event emitted
- **WHEN** the user navigates to the Map view
- **THEN** a `navigation.map` event is recorded (subject to consent)

