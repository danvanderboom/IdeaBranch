## Purpose
Specify the JSON wire format for persisting and restoring `TreeView` state and content.

## Requirements
### Requirement: TreeView JSON serialization format
The system SHALL serialize a `TreeView` as either a standalone view object or wrapped with view root metadata.

#### Scenario: IncludeViewRoot=false emits only the view
- WHEN serialized with `includeViewRoot:false`
- THEN output is the serialized root node object including `IsExpanded`, `ChildrenCount`, and optional `Children`

#### Scenario: IncludeViewRoot=true wraps with metadata
- WHEN serialized with `includeViewRoot:true`
- THEN output contains `RootNodeId`, `IncludedProperties`, `ExcludedProperties`, `DefaultExpanded`, and `View` (the serialized node object)

### Requirement: Node view shape and extras
Each serialized node in a view SHALL include merged payload properties and extras.

#### Scenario: Extras present and ordered
- WHEN a node is serialized into a view
- THEN it contains `NodeId`, `PayloadType`, merged payload properties, `IsExpanded`, `ChildrenCount`, and `Children` only if expanded and non-empty

### Requirement: Property filtering
Serialization SHALL honor `IncludedProperties` (whitelist) and `ExcludedProperties` (blacklist) using full-path matching.

#### Scenario: Included properties restrict output
- WHEN `IncludedProperties` is non-empty
- THEN only properties equal to or under the listed paths are included in the output

#### Scenario: Excluded properties remove output
- WHEN `ExcludedProperties` contains a path
- THEN matching properties are omitted from the output

### Requirement: TreeView JSON deserialization
The system MUST deserialize a `TreeView` using `NodeLookup` to resolve nodes by `NodeId`.

#### Scenario: Restores root and expanded states
- WHEN deserializing a view with `RootNodeId` and nested `View`
- THEN `Root` is set to the looked-up node and per-node expansion is restored

#### Scenario: Restores included/excluded lists
- WHEN deserializing
- THEN `IncludedProperties` and `ExcludedProperties` are restored on the `TreeView`

#### Scenario: Errors on missing root or lookup failure
- WHEN `RootNodeId` is missing or `NodeLookup` returns null for the root
- THEN deserialization throws with an explanatory error


