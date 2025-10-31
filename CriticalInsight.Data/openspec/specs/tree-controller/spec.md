## Purpose
Provide imperative operations for navigating and manipulating trees and their views.

## Requirements
### Requirement: Find nodes by identifier
The controller SHALL locate nodes by `NodeId` using recursive search from `TreeRoot`.

#### Scenario: FindNode returns expected node
- WHEN `FindNode(id)` is invoked for a known node
- THEN the node is returned; otherwise null

### Requirement: Expand/collapse operations
The controller SHALL expand, collapse, toggle individual nodes and entire trees.

#### Scenario: Expand/Collapse/Toggle update view state
- WHEN `ExpandNode`, `CollapseNode`, or `ToggleNode` is called
- THEN the corresponding node's expanded state changes and the view updates

#### Scenario: ExpandAll/CollapseAll affect entire tree
- WHEN `ExpandAll` or `CollapseAll` is called
- THEN all nodes become expanded or collapsed respectively and the view updates

### Requirement: Property filters
The controller SHALL set `IncludedProperties` and `ExcludedProperties` on the view and refresh projection.

#### Scenario: Included/Excluded properties are applied
- WHEN the controller sets included/excluded lists
- THEN the view reflects those lists for subsequent serializations

### Requirement: Node mutation operations
The controller SHALL add, remove, move nodes, and batch these operations, updating the view.

#### Scenario: Add child assigns parent and updates projection
- WHEN `AddChild(parentId, newChild)` is called
- THEN the child is added under the parent, its `Parent` is set, and the view updates

#### Scenario: Remove node(s) update parent and view
- WHEN `RemoveNode(id)` or `RemoveNodes(ids)` are called
- THEN nodes are removed from their parents and the view updates

#### Scenario: Move node reparents correctly
- WHEN `MoveNode(id, newParentId)` is called
- THEN the node is removed from its current parent, `SetParent(newParent)` is used, and the view updates

### Requirement: Payload property updates with safety
The controller MUST update payload properties via reflection while protecting internal node state.

#### Scenario: UpdateNodePayloadProperty sets payload fields
- WHEN `UpdateNodePayloadProperty(id, name, value)` targets a writable payload property
- THEN the property is updated

#### Scenario: Guard against updating internal properties on self-payload nodes
- WHEN the node is self-payloaded
- THEN attempts to modify `NodeId`, `Children`, `Parent`, or `PayloadType` SHALL throw an error

### Requirement: Query helpers
The controller SHALL provide ancestor/descendant enumeration and predicate search.

#### Scenario: GetDescendants/GetAncestors return expected sets
- WHEN called with a valid node id
- THEN results include the correct nodes

#### Scenario: SearchNodes returns matches
- WHEN given a predicate
- THEN all matching nodes in the tree are returned

### Requirement: View export/import
The controller SHALL export a `TreeView` to JSON and import it back, restoring view settings.

#### Scenario: ExportToJson and ImportFromJson restore settings
- WHEN a view is exported (with includeViewRoot) and then imported using a `NodeLookup`
- THEN `IncludedProperties`, `ExcludedProperties`, and expansion states are restored on the target view


