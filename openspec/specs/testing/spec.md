# testing Specification

## Purpose
TBD - created by archiving change add-dotnet-test-data-patterns. Update Purpose after archive.
## Requirements
### Requirement: Test Data Factories/Builders
The system SHALL provide reusable builders to generate valid domain entities with sensible defaults and easy overrides for tests.

#### Scenario: Build valid entity with defaults
- **WHEN** a test uses a domain `*Builder` without overrides
- **THEN** a valid, persistable entity is produced deterministically

#### Scenario: Override specific fields
- **WHEN** a test overrides one or more fields via builder methods
- **THEN** only those fields change; invariants remain valid

#### Scenario: Compose related entities
- **WHEN** a test requests an entity with required relationships
- **THEN** related entities are constructed automatically or via nested builders

### Requirement: Isolated SQLite Test Databases
The test framework MUST isolate data per test using SQLite with either in-memory or per-test file databases, and MUST guarantee cleanup.

#### Scenario: In-memory DB per fixture with transaction-per-test
- **WHEN** tests run within a fixture
- **THEN** a single in-memory SQLite connection is opened once, schema is created once, and each test runs inside a transaction that is rolled back at teardown

#### Scenario: Temp-file DB per test for parallelism
- **WHEN** parallel tests require separate connections
- **THEN** each test uses a unique temp-file SQLite DB that is deleted after teardown

### Requirement: UI Test Cleanup and Artifacts
UI tests SHALL capture artifacts on failure and SHALL reset client/server state between tests.

#### Scenario: Capture screenshot and logs on failure
- **WHEN** a UI test fails
- **THEN** a screenshot and driver/browser logs are saved to a run artifacts directory

#### Scenario: Reset client storage and server state
- **WHEN** a UI test completes
- **THEN** local/session storage is cleared and server test data is reset via a known API or DB rollback

