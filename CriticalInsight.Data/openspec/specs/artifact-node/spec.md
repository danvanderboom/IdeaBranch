## Purpose
Describe the specialized artifact node type and its property bag behaviors.

## Requirements
### Requirement: Artifact node type and propagation
`ArtifactNode` SHALL propagate `ArtifactType` from the root artifact through the subtree.

#### Scenario: Setting ArtifactType on root flows to descendants
- WHEN `ArtifactType` is set on the root `Artifact`
- THEN reading `ArtifactType` on any descendant returns the same value

### Requirement: Typed property bags
Artifact nodes SHALL expose typed property dictionaries and fluent setters.

#### Scenario: Set adds/updates typed properties
- WHEN calling `Set(name, value)` for `string`, `long`, `double`, `decimal`, `DateTime`, `TimeSpan`, or `double[]`
- THEN the corresponding dictionary contains the key/value and subsequent calls update the entry

### Requirement: Fluent tree building helpers
Artifact nodes SHALL support fluent creation of trees.

#### Scenario: SetParent and AddChild return node for chaining
- WHEN `SetParent(parent)` or `AddChild(child)` are called
- THEN the methods return the current node to allow fluent chaining

### Requirement: Artifact serialization support
Artifacts SHALL serialize using the tree/node serialization rules with predefined payload type mappings.

#### Scenario: Serialize and Deserialize artifact
- WHEN an `Artifact` tree is serialized using `Artifact.PayloadTypes` and deserialized via `TreeJsonSerializer`
- THEN round-trip succeeds and node shapes and property bags are preserved

### Requirement: View defaults for artifacts
Artifacts SHALL create a `TreeView` with sensible defaults for display.

#### Scenario: CreateView excludes internal properties
- WHEN `CreateView()` is called
- THEN the resulting view excludes `PayloadType` and `VectorProperties` by default


