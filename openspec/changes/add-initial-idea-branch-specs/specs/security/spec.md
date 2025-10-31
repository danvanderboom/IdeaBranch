## ADDED Requirements

### Requirement: Sensitive data protection
The system MUST protect sensitive user data at rest and in transit using platform-recommended cryptography.

#### Scenario: Transport security
- **WHEN** the app communicates with network services
- **THEN** TLS 1.2+ is enforced and certificate validation is enabled

#### Scenario: Data at rest protection
- **WHEN** storing sensitive data on-device
- **THEN** platform-provided encryption-at-rest mechanisms are used

