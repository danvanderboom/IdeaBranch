## ADDED Requirements

### Requirement: Resilience Policies via Polly
Outbound I/O (e.g., HTTP, storage, messaging) SHALL use Polly-based resilience policies. Retries MUST use exponential backoff with jitter and bounded attempts for transient faults. Non-idempotent operations MUST NOT be retried unless explicitly allowed with safeguards.

#### Scenario: HTTP transient failures
- **WHEN** an HTTP request fails with a transient error (e.g., 408, 429, 5xx except 501/505)
- **THEN** it is retried using an exponential backoff policy with decorrelated jitter up to a configured maximum attempts and total time
- **AND** retry events are logged/emitted with correlation identifiers

#### Scenario: Circuit breaker activation
- **WHEN** consecutive failures exceed thresholds
- **THEN** a circuit breaker opens for a cooldown period and prevents further calls, surfacing a clear error and metrics

#### Scenario: Central policy registration
- **WHEN** configuring dependency injection
- **THEN** a central policy registry is created and used by `HttpClientFactory` handlers and other clients via named policies

#### Scenario: Idempotency guard
- **WHEN** an operation is non-idempotent (e.g., POST that creates a resource)
- **THEN** retries are disabled by default unless explicitly allowed with idempotency keys or equivalent safeguards

