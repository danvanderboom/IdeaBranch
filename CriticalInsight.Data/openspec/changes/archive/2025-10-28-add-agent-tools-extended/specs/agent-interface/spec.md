## ADDED Requirements

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


