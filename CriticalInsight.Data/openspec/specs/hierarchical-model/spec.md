## Purpose
Define the core hierarchical model: nodes, relationships, navigation, events, and disposal.

## Requirements
### Requirement: Tree node structure and relationships
Tree nodes SHALL support hierarchical parent/child relationships and core navigation helpers.

#### Scenario: Create node with children updates depth and root
- WHEN a `TreeNode<T>` is created and children are added
- THEN `Parent` references are set, `Depth` reflects distance from root, and `Root` returns the top-most node

#### Scenario: Moving a node updates parents and depths
- WHEN `SetParent(newParent)` is called on a node
- THEN it is removed from the old parent's `Children`, added to the new parent's `Children`, `Depth` is recalculated, and relevant change events are raised

#### Scenario: Navigation helpers enumerate correctly
- WHEN reading `Ancestors`, `Descendants`, and `Subtree`
- THEN `Ancestors` yields the chain to root, `Descendants` yields a pre-order of all descendants, and `Subtree` yields the node followed by its descendants

### Requirement: Node events propagate appropriately
The model SHALL surface change notifications that bubble through the tree.

#### Scenario: Depth change bubbles to ancestors
- WHEN a node's depth changes (e.g., reparenting)
- THEN `RaiseDepthChangedEvent` triggers `PropertyChanged(Depth)` on the node and bubbles upward to the root

#### Scenario: Ancestor/Descendant change events propagate
- WHEN a node is added or removed
- THEN `RaiseAncestorChangedEvent` notifies all descendants and `RaiseDescendantChangedEvent` bubbles up to the root with the appropriate `NodeChangeType`

### Requirement: Children collection coordinates with parent
`TreeNodeList` SHALL coordinate parent updates when adding/removing.

#### Scenario: Add/Remove keeps Parent consistent
- WHEN `Add(node)` or `Remove(node)` is called
- THEN the child's `Parent` is set/cleared accordingly unless `updateParent:false` is explicitly used

### Requirement: Payload handling
Nodes SHALL maintain a payload object and friendly payload type name.

#### Scenario: Self-payload vs external payload
- WHEN `TreeNode<T>` is constructed
- THEN `PayloadType` is set; if `T` inherits `TreeNode<T>`, the node is self-payloaded (`PayloadObject == this`), otherwise `PayloadObject` is a `new T()` or provided instance

### Requirement: Disposal semantics
Nodes implementing `IDisposable` SHALL dispose payloads and optionally traverse children before or after.

#### Scenario: Dispose honors traversal order
- GIVEN a node whose `Payload` is `IDisposable`
- WHEN `Dispose()` is called
- THEN children are disposed before or after the payload according to `DisposeTraversal` (BottomUp or TopDown)


