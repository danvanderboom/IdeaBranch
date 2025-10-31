## ADDED Requirements

### Requirement: Agent tools surface (in-process API)
The system SHALL expose an agent-callable tools surface (in-process API) with JSON-serializable inputs/outputs that maps to existing model/view/controller operations.

#### Scenario: Read-only tools
- WHEN an agent calls read tools (e.g., get_node, get_view, search, list_children)
- THEN responses are JSON-serializable and include only requested properties subject to filtering limits

#### Scenario: Mutation tools
- WHEN an agent calls mutation tools (e.g., expand_node, collapse_node, add_child, update_payload_property, remove_node, move_node, import_view)
- THEN operations are applied if permitted and validated; outputs include updated identifiers and version tokens

### Requirement: Tool catalog and minimal set
The system MUST provide at least the following tools in v1 with stable names and arguments:

#### Scenario: Core read tools
- WHEN invoked with valid parameters
- THEN the tools succeed:
  - get_node(nodeId, includedProperties?, excludedProperties?)
  - get_view(rootNodeId, includeViewRoot?, defaultExpanded?, includedProperties?, excludedProperties?, depthLimit?, pageSize?, pageToken?)
  - search(rootNodeId, filters[{ path, op(in:[eq,contains]), value }], limit?, pageToken?)
  - list_children(nodeId, limit?, pageToken?)

#### Scenario: Core mutate tools
- WHEN invoked with valid parameters and permissions
- THEN the tools succeed:
  - expand_node(nodeId)
  - collapse_node(nodeId)
  - toggle_node(nodeId)
  - expand_all(rootNodeId)
  - collapse_all(rootNodeId)
  - add_child(parentNodeId, payloadType, payloadProps)
  - update_payload_property(nodeId, propertyName, newValue)
  - remove_node(nodeId)
  - move_node(nodeId, newParentId)
  - export_view(rootNodeId, includeViewRoot?)
  - import_view(viewJson, payloadTypes, rootLookupStrategy)

### Requirement: JSON output shaping and limits
Agent responses SHALL be shaped to control size and avoid token bloat.

#### Scenario: Included/Excluded properties
- WHEN includedProperties is non-empty
- THEN only properties equal to or under the paths are included; excludedProperties removes matches

#### Scenario: Depth and pagination
- WHEN depthLimit and pageSize are provided
- THEN results include at most the requested depth and item count with a continuation pageToken for subsequent calls

### Requirement: Safety and permissions
The system MUST enforce permissions and safe updates for agent interactions.

#### Scenario: Read-only mode
- WHEN operating in read-only mode
- THEN any mutating tool call fails with a forbidden error code

#### Scenario: Role-based permissions
- WHEN an agent identity has role "reader"
- THEN only read tools are allowed; role "editor" allows mutate tools

#### Scenario: Guard internal properties on self-payload nodes
- WHEN update_payload_property targets `NodeId`, `Children`, `Parent`, or `PayloadType` on self-payload nodes
- THEN the call fails with invalid_argument

### Requirement: Concurrency and idempotency
The system SHALL protect against lost updates and duplicate mutations.

#### Scenario: Version tokens required for mutations
- WHEN a mutating tool is invoked without a current version token or with a stale token
- THEN the call fails with conflict and returns the latest version

#### Scenario: Idempotency keys
- WHEN a mutating tool is invoked with an idempotencyKey already used for an identical request
- THEN the prior successful result is returned and no duplicate changes occur

### Requirement: Rate limiting and batch limits
The system SHALL limit resource usage for agent calls.

#### Scenario: Per-agent rate limit
- WHEN an agent exceeds the configured rate limit
- THEN calls fail with rate_limited including a retryAfter hint

#### Scenario: Batch size limits
- WHEN a tool accepts multiple identifiers
- THEN a maximum batch size is enforced and larger requests fail with invalid_argument

### Requirement: Audit logging
The system MUST record auditable entries for mutating agent actions.

#### Scenario: Audit record created for mutations
- WHEN a mutating tool call completes (success or failure)
- THEN an audit entry is recorded including timestamp, agent identity, operation, targets, and outcome

### Requirement: Error model
Agent tools MUST return structured errors with machine-readable codes.

#### Scenario: Standard error codes
- WHEN a tool fails
- THEN error responses include one of: invalid_argument, not_found, forbidden, conflict, rate_limited, internal, with a human-readable message

### Requirement: Transport-agnostic contract with in-process API
The agent interface SHALL be transport-agnostic and MUST provide an in-process API in v1.

#### Scenario: In-process API availability
- WHEN linking the library in-process
- THEN the tools surface is callable without network transport; external transport gateways may be added without changing tool semantics


