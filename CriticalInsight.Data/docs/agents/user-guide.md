## Agent Interface — User Guide

This guide introduces the Agent API tools for working with hierarchical trees. It focuses on what you can do and provides simple examples.

### Concepts

- A tree is made of nodes; each node has a payload (like a "Space" with `Name` and `SquareFeet`).
- A view is a flattened list of visible nodes (depends on expansion and filters).
- Tools are grouped as: navigate, search, edit, organize, validate, and snapshot.

### Navigation & Retrieval

- `GetPath(nodeId)`: Get breadcrumbs from root to the node
- `GetSubtree(nodeId, depthLimit)`: Get a node and its children (optionally limited by depth)
- `GetCommonAncestor(nodeIds)`: Find the nearest shared ancestor

```csharp
var path = svc.GetPath(ctx, nodeId);
var subtree = svc.GetSubtree(ctx, nodeId, new ViewOptions { MaxDepth = 2 });
```

### Search & Selection

- Quick search: `Search(ctx, rootId, filters, paging)`
- Advanced: `SearchAdvanced(options)` with predicates and sorting
- DSL selection: `SelectNodes("Name contains Kitchen AND SquareFeet > 100")`

```csharp
var results = svc.SelectNodes(ctx, rootId, new SelectQuery { Expression = "Name contains Kitchen" }, new PageOptions { PageSize = 25 });
```

### Structural Editing

- Copy/clone: `CopySubtree`, `CloneNode`
- Move/reorder: `MoveBefore`, `MoveAfter`, `SortChildren`

```csharp
var copyId = svc.CopySubtree(ctxEditor, sourceId, targetParentId, new CopyOptions { Duplicate = true }, new MutationOptions());
```

### Bulk Update

- Update properties on one node: `UpdatePayload`
- Update many nodes: `UpdateNodes`

```csharp
var r = svc.UpdatePayload(editorCtx, nodeId, new Dictionary<string, object?> { ["Name"] = "Kitchen" }, new MutationOptions());
```

### View Control

- Expand/collapse recursively: `SetExpansionRecursive`
- Include/exclude properties: `SetFilters`

```csharp
svc.SetExpansionRecursive(editorCtx, rootId, new ExpansionOptions { Expanded = true, MaxDepth = 2 }, new MutationOptions());
```

### Tagging & Bookmarks

- Tags: `AddTags`, `RemoveTags`, `GetTags`, `FindNodesByTag`
- Bookmarks: `CreateBookmark`, `ListBookmarks`, `DeleteBookmark`

```csharp
svc.AddTags(editorCtx, nodeId, new TagOptions { Tags = new() { "important" } }, new MutationOptions());
```

### Validation & Diff

- Check a tree or node: `ValidateTree`, `ValidateNode`
- Compare trees: `DiffTrees` (structure, payloads, metadata)

```csharp
var issues = svc.ValidateTree(ctx, rootId, new ValidationOptions { CheckStructure = true, CheckPayloads = true });
```

### Snapshots

- Save state: `CreateSnapshot` (includes optional view state, tags, bookmarks)
- Restore: `RestoreSnapshot` (see note below)
- List and manage: `ListSnapshots`, `GetSnapshot`, `DeleteSnapshot`

```csharp
var snapId = svc.CreateSnapshot(editorCtx, rootId, new SnapshotOptions { Name = "Before Sort" }, new MutationOptions());
var snapshots = svc.ListSnapshots(editorCtx);
var snapshotData = svc.GetSnapshot(editorCtx, snapId);
```

Note: Current implementation returns an error for `RestoreSnapshot` due to internal design constraints; restoring requires creating a new service with the restored root.

### Multi-Format Export/Import

- Export to multiple formats: `ExportTree`, `ExportToFormat` (JSON, XML, CSV)
- Import with validation: `ImportTree`
- Get export metadata: `GetExportMetadata`

```csharp
// Export to different formats
var jsonExport = svc.ExportTree(editorCtx, rootId, new ExportOptions { Format = "json" });
var xmlExport = svc.ExportToFormat(editorCtx, rootId, "xml", new ExportOptions());
var csvExport = svc.ExportToFormat(editorCtx, rootId, "csv", new ExportOptions());

// Get metadata about the tree
var metadata = svc.GetExportMetadata(editorCtx, rootId);
```

### Safety

- Read‑only mode and roles restrict edits
- Idempotency: repeat a mutation safely with the same idempotency key
- Versioning: provide expected version to avoid overwriting changes
- Rate limiting: calls may be throttled; check `RetryAfter`

### Error Handling

Most calls return `AgentResult<T>`:
- `Success == true` → `Data` contains the result
- `Success == false` → inspect `Error.Code` (e.g., `forbidden`, `conflict`, `rate_limited`, `validation_failed`)

### Tips

- Use paging (`PageOptions`) for large results
- Apply depth limits when exporting or listing children
- Prefer bulk updates for many small edits; request `ContinueOnError` as needed


