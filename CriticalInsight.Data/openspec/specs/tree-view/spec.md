## Purpose
Provide a flattened, filterable projection over a hierarchical tree with per-node expansion state.

## Requirements
### Requirement: Expanded state per node
The view SHALL track expanded/collapsed state for every node with a configurable default.

#### Scenario: Get/Set expanded state
- WHEN `SetIsExpanded(node, bool)` is called
- THEN `GetIsExpanded(node)` reflects the value and the view is refreshed

#### Scenario: Default expanded behavior
- WHEN a view is created with `defaultExpanded:true`
- THEN all nodes are considered expanded unless explicitly collapsed

### Requirement: Flattened projected collection
The view SHALL maintain a flattened `ProjectedCollection` of visible nodes excluding the root.

#### Scenario: Fully expanded shows all descendants
- WHEN all nodes are expanded
- THEN `ProjectedCollection` lists nodes in pre-order, excluding the root

#### Scenario: Collapsing hides descendants
- WHEN a node is collapsed
- THEN that node remains visible but its descendants are removed from `ProjectedCollection`

#### Scenario: Collapsing root hides all
- WHEN the root is collapsed
- THEN `ProjectedCollection` is empty

#### Scenario: Leaf collapse does not change count
- WHEN a leaf node is collapsed
- THEN `ProjectedCollection` is unchanged because leaves have no visible children

### Requirement: Dynamic updates
The view SHALL react to children adds/removes and property changes by incrementally updating the projection.

#### Scenario: Add child under visible parent updates projection
- WHEN a child is added to a visible (expanded) parent
- THEN the new node appears in `ProjectedCollection` at the correct position

#### Scenario: Remove visible node updates projection
- WHEN a visible node is removed
- THEN it disappears from `ProjectedCollection`

### Requirement: Multiple independent views
Multiple `TreeView` instances over the same tree SHALL maintain independent expanded state and projections.

#### Scenario: Independent projections per view
- WHEN one view collapses a node
- THEN another view over the same root remains unaffected

### Requirement: Visibility helper
The view SHALL report if a node is visible under current expansion.

#### Scenario: IsNodeVisible respects ancestor expansion
- WHEN `IsNodeVisible(node)` is queried
- THEN it returns false if any ancestor is collapsed and true otherwise


