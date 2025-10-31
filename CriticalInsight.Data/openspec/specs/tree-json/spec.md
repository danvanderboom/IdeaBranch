## Purpose
Define the JSON format and behaviors for serializing/deserializing `ITreeNode` trees.

## Requirements
### Requirement: Node JSON serialization
The system SHALL serialize `ITreeNode` instances with stable ordering and payload handling.

#### Scenario: Required properties and exclusions
- WHEN a node is serialized
- THEN output includes `NodeId`, `PayloadType`, `Children`
- AND properties such as `Root`, `Parent`, `Depth`, `Ancestors`, `Descendants`, `Subtree` are not serialized

#### Scenario: Self-payload nodes inline their properties
- GIVEN a node where `PayloadObject == node`
- WHEN serialized
- THEN payload properties appear inline (excluding `NodeId`, `PayloadType`, `Children`, `Payload`), and no `Payload` object is emitted

#### Scenario: External payload uses Payload object
- GIVEN a node where `PayloadObject` is not the node
- WHEN serialized
- THEN a `Payload` object is emitted containing the payload's properties

#### Scenario: Children are serialized last
- WHEN a node with children is serialized
- THEN `Children` is a JSON array of serialized child nodes in traversal order

### Requirement: Friendly payload type names
The system SHALL support mapping between friendly payload type names and runtime `Type`.

#### Scenario: Friendly names round-trip
- GIVEN a mapping `{ name -> Type }`
- WHEN serializing and deserializing
- THEN `PayloadType` uses friendly names where provided, and round-tripping yields byte-for-byte identical JSON for the same tree shape and payloads

### Requirement: Node JSON deserialization
The system MUST deserialize a node from JSON using payload type mapping.

#### Scenario: Valid payload type deserializes
- WHEN `PayloadType` resolves to a known type (via mapping or `Type.GetType`)
- THEN the node instance is created; if self-payload, inline properties populate the node; otherwise the `Payload` object deserializes and is assigned

#### Scenario: Children deserialize recursively
- WHEN a node JSON contains a `Children` array
- THEN each child is deserialized as `ITreeNode` and added to the parent's `Children`

#### Scenario: Invalid or missing payload type fails
- WHEN `PayloadType` is missing or cannot be resolved
- THEN deserialization fails with a JSON exception


