# Agent Interface API

The Agent Interface provides a safe, paginated, and auditable API for AI agents and automation to interact with hierarchical tree data structures.

For detailed guides:
- Developer-focused: `docs/agents/developer-guide.md`
- User-focused: `docs/agents/user-guide.md`

## Quick Start

```csharp
using CriticalInsight.Data.Agents;
using CriticalInsight.Data.Hierarchical;

// Create a tree and view
var root = new TreeNode<Space>(new Space { Name = "Property", SquareFeet = 20000 });
var view = new TreeView(root);

// Define payload types for serialization
var payloadTypes = new Dictionary<string, Type>
{
    { nameof(Space), typeof(Space) },
    { nameof(Substance), typeof(Substance) }
};

// Create agent service
var svc = new AgentTreeService(root, view, payloadTypes);

// Create agent context (Editor role allows mutations)
var ctx = new AgentContext("agent-1", readOnly: false, new[] { AgentRole.Editor });

// Read operations
var nodeJson = svc.GetNode(ctx, root.NodeId, new PropertyFilters());
var viewJson = svc.GetView(ctx, root.NodeId, new ViewOptions { DepthLimit = 3 });
var children = svc.ListChildren(ctx, root.NodeId, new PageOptions { PageSize = 10 });

// Mutation operations (require Editor role)
var result = svc.AddChild(ctx, root.NodeId, nameof(Space), 
    new Dictionary<string, object?> { { "Name", "House" }, { "SquareFeet", 2500d } },
    new MutationOptions());
```

## Key Features

### Safety & Permissions
- **Read-only mode**: Prevents all mutations
- **Role-based access**: `Reader` (read-only) vs `Editor` (read + mutate)
- **Internal property guards**: Prevents modification of `NodeId`, `Children`, `Parent`, `PayloadType` on self-payload nodes

### Output Shaping
- **Property filtering**: Include/exclude specific properties using dot-notation paths
- **Depth limiting**: Limit tree depth in `GetView` responses
- **Pagination**: Token-based pagination for `ListChildren` and `Search`

### Concurrency & Reliability
- **Version tokens**: Required for mutations to prevent lost updates
- **Idempotency keys**: Deduplicate identical mutations within 10-minute TTL
- **Rate limiting**: Token bucket per agent (default: 60 ops/minute)

### Observability
- **Audit logging**: All mutations logged with agent ID, operation, target, and outcome
- **Structured errors**: Machine-readable error codes (`invalid_argument`, `not_found`, `forbidden`, `conflict`, `rate_limited`, `internal_error`)

## API Reference

### Core Read Operations
- `GetNode(ctx, nodeId, filters)` - Serialize single node
- `GetView(ctx, rootId, options)` - Serialize tree view with depth/expansion control
- `ListChildren(ctx, nodeId, paging)` - Paginated children list
- `Search(ctx, rootId, filters, paging)` - Filtered search with pagination

### Core Mutation Operations
- `ExpandNode/CollapseNode/ToggleNode(ctx, nodeId, opts)` - Control expansion
- `ExpandAll/CollapseAll(ctx, rootId, opts)` - Bulk expansion control
- `AddChild(ctx, parentId, payloadType, props, opts)` - Add new child node
- `UpdatePayloadProperty(ctx, nodeId, name, value, opts)` - Update node property
- `RemoveNode(ctx, nodeId, opts)` - Remove node from tree
- `MoveNode(ctx, nodeId, newParentId, opts)` - Reparent node
- `ExportView(ctx, rootId, includeViewRoot)` - Export view state
- `ImportView(ctx, json, payloadTypes, nodeLookup, opts)` - Import view state

### Extended Tools (15+ functions)

#### Navigation & Retrieval
- `GetPath(ctx, nodeId)` - Get breadcrumb path from root to node
- `GetSubtree(ctx, nodeId, depthLimit, filters, paging)` - Get subtree with depth/property filtering
- `GetCommonAncestor(ctx, nodeIds)` - Find nearest common ancestor

#### Advanced Search & Selection
- `SearchAdvanced(ctx, rootId, options, paging)` - Complex search with predicates and sorting
- `SelectNodes(ctx, rootId, query, paging)` - DSL-based node selection

#### Structural Editing
- `CopySubtree(ctx, sourceId, targetParentId, options, opts)` - Copy subtree (reference or duplicate)
- `CloneNode(ctx, nodeId, targetParentId, opts)` - Clone single node
- `MoveBefore/MoveAfter(ctx, nodeId, siblingId, opts)` - Precise sibling reordering
- `SortChildren(ctx, parentId, options, opts)` - Sort children by property

#### Bulk Operations
- `UpdatePayload(ctx, nodeId, properties, opts)` - Update multiple properties on one node
- `UpdateNodes(ctx, updates, options, opts)` - Bulk update multiple nodes

#### View Control
- `SetExpansionRecursive(ctx, rootId, options, opts)` - Recursive expansion control
- `SetFilters(ctx, options, opts)` - Set property include/exclude filters

#### Tagging & Bookmarks
- `AddTags/RemoveTags/GetTags(ctx, nodeId, options, opts)` - Tag management
- `FindNodesByTag(ctx, rootId, tag, paging)` - Find nodes by tag
- `CreateBookmark/ListBookmarks/DeleteBookmark(ctx, options, opts)` - Bookmark management

#### Validation & Diff
- `ValidateTree/ValidateNode(ctx, rootId, options)` - Structure and payload validation
- `DiffTrees(ctx, rootId1, rootId2, options)` - Compare trees (structure, payloads, metadata)

#### Snapshots
- `CreateSnapshot/ListSnapshots/GetSnapshot/DeleteSnapshot(ctx, options, opts)` - Snapshot management
- `RestoreSnapshot(ctx, snapshotId, options, opts)` - Restore from snapshot (see limitations)

#### Multi-Format Export/Import
- `ExportTree(ctx, rootId, options)` - Export complete tree with metadata
- `ImportTree(ctx, jsonData, options, opts)` - Import tree with validation
- `ExportToFormat(ctx, rootId, format, options)` - Export to JSON/XML/CSV
- `GetExportMetadata(ctx, rootId)` - Get tree statistics and metadata

### Configuration Options
- **Rate limiting**: Pass custom `IRateLimiter` to constructor
- **Audit logging**: Pass custom `IAuditLogger` (default: no-op)
- **Idempotency**: Pass custom `IIdempotencyStore` (default: in-memory)
- **Versioning**: Pass custom `IVersionProvider` (default: in-memory)

## Error Handling

All operations return `AgentResult<T>` with success/error information:

```csharp
var result = svc.GetNode(ctx, nodeId, filters);
if (result.Success)
{
    var nodeJson = result.Data;
}
else
{
    var error = result.Error;
    // error.Code: AgentErrorCode enum
    // error.Message: Human-readable message
    // error.RetryAfter: Optional retry delay hint
}
```

## Best Practices

1. **Always check version tokens** for mutations to handle concurrent updates
2. **Use idempotency keys** for retry-safe operations
3. **Apply property filters** to reduce response size and token usage
4. **Use depth limits** for large trees to prevent overwhelming responses
5. **Handle rate limiting** with exponential backoff using `RetryAfter` hints
6. **Monitor audit logs** for security and compliance

## Transport Notes

This v1 implementation provides an in-process API. Future versions may add HTTP/gRPC transport layers without changing the core tool semantics.
