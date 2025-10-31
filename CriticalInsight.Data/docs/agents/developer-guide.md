## Agent Interface v1 — Developer Guide

This guide explains the architecture, behaviors, and extension points of the in‑process Agent API that operates over the hierarchical tree model/view/controller.

### Overview

- Entry point: `CriticalInsight.Data.Agents.IAgentTreeService`
- Implementation: `CriticalInsight.Data.Agents.AgentTreeService`
- Tree core: `ITreeNode`, `TreeNode<T>`, `TreeView`, `TreeController<ITreeNode>`
- Serialization: `TreeJsonSerializer`, `TreeViewJsonSerializer`
- Cross‑cutting: `AgentContext` (roles, read‑only), `IAuditLogger`, `IRateLimiter`, `IIdempotencyStore`, `IVersionProvider`

### Key Capabilities

#### Core Operations
- **Read/navigation**: `GetNode`, `GetView`, `ListChildren`, `Search`
- **Basic mutations**: `ExpandNode/CollapseNode/ToggleNode`, `AddChild`, `UpdatePayloadProperty`, `RemoveNode`, `MoveNode`
- **Import/Export**: `ExportView`, `ImportView`

#### Extended Tools (15+ functions)
- **Navigation**: `GetPath`, `GetSubtree`, `GetCommonAncestor`
- **Advanced search/selection**: `SearchAdvanced`, `SelectNodes` (with DSL support)
- **Structural editing**: `CopySubtree`, `CloneNode`, `MoveBefore/After`, `SortChildren`
- **Bulk operations**: `UpdatePayload`, `UpdateNodes` (with error handling)
- **View control**: `SetExpansionRecursive`, `SetFilters`
- **Tagging & bookmarks**: `AddTags/RemoveTags/GetTags/FindNodesByTag`, `CreateBookmark/ListBookmarks/DeleteBookmark`
- **Validation & diff**: `ValidateTree/ValidateNode`, `DiffTrees` (structure, payloads, metadata)
- **Snapshots**: `CreateSnapshot/ListSnapshots/GetSnapshot/RestoreSnapshot/DeleteSnapshot`
- **Multi-format export**: `ExportTree`, `ImportTree`, `ExportToFormat` (JSON, XML, CSV), `GetExportMetadata`

### Safety & Guarding

All service calls go through a guard that enforces:
- Rate limits (per `AgentContext.AgentId`)
- Authorization (`AgentRole.Reader` vs `AgentRole.Editor` and `ReadOnly` flag)
- Idempotency (for mutations, when `MutationOptions.IdempotencyKey` is provided)
- Optimistic concurrency via `IVersionProvider` tokens on mutations
- Audit logging for mutations (before/after summary)

Error model uses `AgentResult<T>` with `AgentError { Code, Message, RetryAfter? }` and well‑defined `AgentErrorCode` values (e.g., `forbidden`, `conflict`, `rate_limited`, `validation_failed`).

### Output Shaping

- Property filtering: include/exclude property names on views
- Depth limits: applied to `GetView` and `GetSubtree`
- Pagination: pre‑order stable traversal; `PageOptions.PageToken` is a base64 cursor of a start index

### Concurrency, Idempotency, Rate Limits

- Concurrency: return a version token on reads that mutate calls can assert
- Idempotency: cache successful mutation results (per agentId + key) for TTL; replays return the cached result
- Rate limiting: token bucket per agent; `RetryAfter` included when throttled

### Validation & Diff

- `ValidateTree/ValidateNode(ValidationOptions)`: structural checks, payload presence/required properties, parent/depth consistency
- `DiffTrees(DiffOptions)`: structural diffs, payload property diffs, and (optionally) metadata diffs (e.g., tags)

### Snapshots

- `CreateSnapshot(SnapshotOptions)`: captures serialized tree plus optional view state, tags, and bookmarks (agent‑scoped)
- `RestoreSnapshot(RestoreOptions)`: note the current design constraint—service fields are readonly; restoring requires creating a new service instance with the restored root. The current implementation returns a descriptive error instead of mutating internal references.

### Extension Points

- Add new tools by extending `IAgentTreeService` and implementing in `AgentTreeService`
- Add DTOs in `AgentDtos.cs` for inputs/outputs and options
- Use existing helpers for paging, depth limiting, and filtering to ensure consistent behavior

### Testing

#### Unit Tests
- Unit tests under `CriticalInsight.Data.UnitTests/Agents/` provide comprehensive coverage:
  - **Core Operations**: Navigation, search/selection, structural editing, bulk update
  - **View Control**: Expansion/collapse, filtering, pagination with token hardening
  - **Safety Guards**: Role-based authorization, read-only precedence, self-payload property protection
  - **Concurrency Controls**: Rate limiting (per-agent isolation), idempotency (TTL expiry), version conflicts
  - **Audit Logging**: Success/failure tracking with error codes and messages
  - **Error Handling**: Malformed pagination tokens, invalid property updates, forbidden operations
  - **Cross-cutting Concerns**: TypeExtensions formatting, ICollectionExtensions boundary conditions
  - **All 168 tests pass** with comprehensive coverage of critical safety and concurrency paths

#### Test Coverage Improvements (Latest)

Recent enhancements have significantly strengthened test coverage for critical safety and concurrency paths:

- **Self-Payload Property Guards**: Tests verify that attempts to modify internal node properties (NodeId, Children, Parent, PayloadType) on self-payload nodes like `ArtifactNode` are properly rejected with `invalid_argument` errors
- **Read-Only Precedence**: Tests confirm that `ReadOnly=true` overrides role-based permissions, ensuring read-only contexts cannot perform mutations even with Editor role
- **Pagination Token Hardening**: Tests validate graceful handling of malformed pagination tokens, ensuring robust behavior with corrupted or invalid base64-encoded cursors
- **Audit Logging on Failures**: Tests verify that all guarded operation failures (rate limiting, authorization, version conflicts) are properly logged with appropriate error codes and messages
- **Concurrency Primitive Testing**: Comprehensive tests for rate limiting (per-agent isolation, refill behavior), idempotency (TTL expiry, agent scoping), and versioning (conflict detection, monotonic bumping)
- **Extension Method Coverage**: Tests for `TypeExtensions` generic type formatting and `ICollectionExtensions` boundary condition handling

These improvements ensure robust error handling, security enforcement, and reliable concurrency behavior across all agent operations.

#### Integration Tests
- Integration tests under `CriticalInsight.Data.IntegrationTests/Agents/Integration/` provide:
  - Agent Framework integration with Azure OpenAI and LM Studio
  - Hierarchical context evaluation with different view variants
  - MCQ-based decision making tests with success rate assertions
  - PowerShell scripts for easy test execution and LM Studio management
  - Environment variable configuration for different providers and settings

### Known Limitations

- Snapshot restoration currently returns an error due to readonly internals; plan to support recreation or refactor to enable safe in‑place restoration.

### Example: Guarded Mutation

```csharp
var result = svc.UpdatePayload(editorCtx, nodeId, new Dictionary<string, object?>
{
    ["Name"] = "Kitchen",
    ["SquareFeet"] = 225
}, new MutationOptions { IdempotencyKey = "edit-123", ExpectedVersion = version });

if (!result.Success)
{
    // inspect result.Error.Code (e.g., forbidden, conflict, rate_limited)
}
```


