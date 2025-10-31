# agent-interface Specification

## Purpose
Define a safe, compact, and testable interface for agents (LLMs and automation) to read and manipulate hierarchical trees and their views. This specification covers agent-callable operations, safety requirements, and observability needs.
## Requirements
### Requirement: Core Agent Operations
The system SHALL provide agent-callable operations for reading and manipulating hierarchical data through a JSON-serializable interface.

#### Scenario: Read tree structure
- **WHEN** an agent requests tree data with specified depth and property filters
- **THEN** the system returns JSON-serialized tree view with requested scope

#### Scenario: Search and navigation
- **WHEN** an agent searches for nodes by criteria or navigates by path
- **THEN** the system returns matching nodes with their hierarchical context

#### Scenario: Expand and collapse operations
- **WHEN** an agent requests to expand or collapse specific tree branches
- **THEN** the system updates the view state and returns the modified structure

#### Scenario: Mutate tree data
- **WHEN** an agent requests to update node properties or structure
- **THEN** the system applies changes with proper validation and returns updated state

### Requirement: Safety and Security
The system SHALL enforce safety requirements including read-only mode, property guards, role-based permissions, and operational limits.

#### Scenario: Read-only mode enforcement
- **WHEN** an agent attempts to modify data in read-only mode
- **THEN** the system rejects the operation and returns an appropriate error

#### Scenario: Property update guards
- **WHEN** an agent attempts to modify protected properties
- **THEN** the system validates permissions and blocks unauthorized changes

#### Scenario: Rate limiting
- **WHEN** an agent exceeds configured rate limits
- **THEN** the system throttles requests and returns rate limit exceeded errors

#### Scenario: Batch size limits
- **WHEN** an agent requests operations exceeding batch size limits
- **THEN** the system rejects the request and suggests appropriate batch sizes

### Requirement: Concurrency Control
The system SHALL provide concurrency control through version tokens and idempotency keys for mutating operations.

#### Scenario: Version token validation
- **WHEN** an agent submits a mutation with an outdated version token
- **THEN** the system rejects the operation and returns current version information

#### Scenario: Idempotency key handling
- **WHEN** an agent submits duplicate operations with the same idempotency key
- **THEN** the system returns the result of the original operation without side effects

### Requirement: Observability and Audit
The system SHALL provide audit logging for all agent actions and operations.

#### Scenario: Agent action logging
- **WHEN** an agent performs any operation
- **THEN** the system logs the action with agent identifier, operation type, and relevant context

#### Scenario: Error tracking
- **WHEN** an agent operation fails
- **THEN** the system logs the error details with sufficient context for debugging

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

### Requirement: Path retrieval
Agents SHALL retrieve the breadcrumb path from root to a node.

#### Scenario: get_path returns nodeIds and names
- WHEN `get_path(nodeId)` is called
- THEN the response includes an ordered list from root to the node with `{ nodeId, name?, depth }`

### Requirement: Subtree retrieval with pruning
Agents SHALL fetch a subtree with optional depth and property filtering.

#### Scenario: get_subtree respects depth and filters
- WHEN `get_subtree(nodeId, depthLimit, filters, paging)` is called
- THEN the returned structure includes only nodes within depth and respects included/excluded properties and paging

### Requirement: Common ancestor
Agents SHALL compute the nearest common ancestor of a set of nodes.

#### Scenario: get_common_ancestor returns nearest ancestor
- WHEN `get_common_ancestor(nodeIds[])` is called
- THEN the nearest common ancestor's `nodeId` is returned or null if none

### Requirement: Advanced search
Agents SHALL search using path-based predicates and basic sort.

#### Scenario: search_advanced supports AND/OR and ranges
- WHEN `search_advanced(rootId, predicates, sortBy?, limit?, pageToken?)` is called
- THEN results honor predicates (`eq`, `contains`, `gt`, `lt`, `between`) with AND/OR groups and optional sort and pagination

### Requirement: Node selection DSL
Agents SHALL be able to select nodes using a compact DSL.

#### Scenario: select_nodes parses simple expressions
- WHEN `select_nodes(queryDsl)` targets by payload properties and ancestor paths
- THEN matching nodes are returned in a deterministic order

### Requirement: Subtree copy and node clone
Agents SHALL duplicate structures locally in memory.

#### Scenario: copy_subtree duplicates or references children
- WHEN `copy_subtree(sourceId, targetParentId, mode: reference|duplicate)` is called
- THEN a new subtree is attached under the target; in `reference` mode payloads are shared, in `duplicate` mode payloads are deep-copied when possible

#### Scenario: clone_node duplicates a single node
- WHEN `clone_node(nodeId, targetParentId)` is called
- THEN a new node with the same payload is added under the target

### Requirement: Precise reordering
Agents SHALL precisely reorder nodes among siblings.

#### Scenario: move_before/move_after adjust sibling order
- WHEN `move_before(nodeId, siblingId)` or `move_after(nodeId, siblingId)` is called
- THEN the node is placed immediately before/after the sibling within the same parent

### Requirement: Child sorting
Agents SHALL sort children of a parent by a property.

#### Scenario: sort_children applies stable ordering
- WHEN `sort_children(parentId, byProp, direction, stable?)` is called
- THEN children are ordered accordingly and stability is preserved when requested

### Requirement: Multi-field payload update
Agents SHALL update multiple payload properties in one call.

#### Scenario: update_payload patches multiple fields
- WHEN `update_payload(nodeId, patch)` is called
- THEN all settable fields in `patch` are updated on the payload; guarded internal properties remain protected

### Requirement: Bulk updates by predicate
Agents SHALL update multiple nodes matched by a local predicate without transactional guarantees.

#### Scenario: update_nodes applies non-atomic bulk changes
- WHEN `update_nodes(filter, patch)` is called
- THEN each matching node is updated independently without atomic transaction or rollback

### Requirement: Recursive view expansion control
Agents SHALL control expanded state recursively with an optional depth.

#### Scenario: set_expansion_recursive with depth
- WHEN `set_expansion_recursive(nodeId, expanded, maxDepth?)` is called
- THEN nodes under the subtree are expanded/collapsed up to the specified depth

### Requirement: View filter presets
Agents SHALL set include/exclude property filters for subsequent serializations.

#### Scenario: set_filters persists filters
- WHEN `set_filters(scope, included[], excluded[])` is called
- THEN those filters are applied for later `GetView`/`GetNode` calls in the same agent session

### Requirement: Tagging and bookmarks
Agents SHALL tag nodes and manage bookmarks for quick navigation.

#### Scenario: tag and query by tag
- WHEN `tag_node(nodeId, tags[])` and `query_by_tag(tag)` are used
- THEN tags are associated in memory and queries return tagged nodes

#### Scenario: bookmark and list/remove bookmarks
- WHEN `bookmark(nodeId, label)` then `list_bookmarks()` or `remove_bookmark(label)` are called
- THEN bookmarks are stored in memory, listable, and removable

### Requirement: Validation and linting
Agents SHALL validate tree structure and lint payloads using local rules.

#### Scenario: validate_tree checks structure
- WHEN `validate_tree(rulesetId)` is executed locally
- THEN violations (e.g., missing required fields, invalid relationships) are returned

#### Scenario: lint_payload checks payload formatting
- WHEN `lint_payload(nodeId, rulesetId)` is executed locally
- THEN any payload issues are reported with paths and messages

### Requirement: Snapshots and diff (in-memory)
Agents SHALL manage in-memory snapshots and diffs of the tree.

#### Scenario: snapshot_create/list/restore
- WHEN a snapshot is created with `snapshot_create(label)` and later restored by id
- THEN the current in-memory tree is saved/restored without persistence guarantees

#### Scenario: snapshot_diff and diff_subtree
- WHEN `snapshot_diff(a,b)` or `diff_subtree(aId,bId)` are called
- THEN a structural and payload diff is returned for local comparison

### Requirement: Export/import enhancements
Agents SHALL export a subtree and merge imports locally.

#### Scenario: export_subtree serializes subtree
- WHEN `export_subtree(nodeId, includeViewRoot?)` is called
- THEN a JSON representation of the subtree is returned using existing serialization rules

#### Scenario: import_merge supports strategies
- WHEN `import_merge(json, strategy: upsert|replace|skip, matchBy: nodeId|path)` is executed
- THEN nodes are merged into the current tree using the specified strategy without external connectivity

