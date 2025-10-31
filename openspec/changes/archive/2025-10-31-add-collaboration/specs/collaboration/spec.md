## ADDED Requirements

### Requirement: Team creation and management
The system SHALL allow users to create and manage teams for collaborative research.

#### Scenario: Create team
- **WHEN** a user creates a team
- **THEN** the team starts with an empty list of topics
- **AND** the creator is the default owner and a team admin

#### Scenario: Invite users to team
- **WHEN** a team admin invites users to join the team
- **THEN** invited users receive invitations
- **AND** upon acceptance, users are added to the team

#### Scenario: Import topics to team
- **WHEN** a team admin imports existing topics
- **THEN** topics belonging to other users or teams can be imported
- **AND** imported topics are available to the team

### Requirement: Share topics with other users
The system SHALL allow users to share their private topics with other users.

#### Scenario: Share topic with user
- **WHEN** a user shares a private topic with another user
- **THEN** the invitee receives a notification
- **AND** upon acceptance, the invitee can access the topic according to granted access level

### Requirement: Access levels and permissions
The system SHALL support different access levels for shared topics and team content, similar to Google Docs permissions.

#### Scenario: View-only access
- **WHEN** a user is granted view-only access to a topic
- **THEN** the user can view the topic and its content
- **AND** the user cannot edit, annotate, or delete content

#### Scenario: View and annotate access
- **WHEN** a user is granted view and annotate access to a topic
- **THEN** the user can view and create annotations on the topic
- **AND** the user cannot edit topic nodes or delete content

#### Scenario: Edit access
- **WHEN** a user is granted edit access to a topic
- **THEN** the user can edit topic nodes and create annotations
- **AND** the user cannot delete pinned/locked nodes (unless granted additional permissions)

#### Scenario: Manage permissions
- **WHEN** a topic owner manages permissions
- **THEN** access levels can be changed for existing invitees
- **AND** new users can be invited with specified access levels

### Requirement: Node pinning and locking
The system SHALL allow topic owners to pin or lock specific nodes to prevent deletion or modification.

#### Scenario: Pin node
- **WHEN** a topic owner pins a node
- **THEN** the node cannot be deleted by users with edit access
- **AND** the pinned status is visually indicated

#### Scenario: Lock node
- **WHEN** a topic owner locks a node
- **THEN** the node cannot be edited, moved, or deleted by users with edit access
- **AND** the locked status is visually indicated

### Requirement: Real-time collaborative editing
The system SHALL support real-time collaborative editing of topic hierarchies, prompt template trees, and tag taxonomies.

#### Scenario: Real-time edits to topic hierarchies
- **WHEN** multiple users edit the same topic hierarchy concurrently
- **THEN** changes are synchronized and visible to all connected users in real-time
- **AND** edits are subscribed to and broadcast to other users

#### Scenario: Real-time edits to prompt templates
- **WHEN** multiple users edit the same prompt template tree concurrently
- **THEN** changes are synchronized and visible to all connected users in real-time
- **AND** edits are subscribed to and broadcast to other users

#### Scenario: Real-time edits to tag taxonomies
- **WHEN** multiple users edit the same tag taxonomy concurrently
- **THEN** changes are synchronized and visible to all connected users in real-time
- **AND** edits are subscribed to and broadcast to other users

#### Scenario: Show active users
- **WHEN** multiple users are viewing or editing the same content
- **THEN** active users are indicated (e.g., with user indicators or cursors)
- **AND** users can see who else is working on the content

#### Scenario: Real-time collaboration for team topics
- **WHEN** team members edit team-owned topics
- **THEN** real-time collaboration works for team topics
- **AND** changes are synchronized across all team members viewing the content

#### Scenario: Real-time collaboration for individual topics
- **WHEN** invited users edit individually-owned topics
- **THEN** real-time collaboration works for shared individual topics
- **AND** changes are synchronized across all users with access

### Requirement: Conflict resolution
The system SHALL handle concurrent edits with appropriate conflict resolution strategies.

#### Scenario: Handle concurrent edits
- **WHEN** multiple users edit the same node concurrently
- **THEN** the system applies a conflict resolution strategy (e.g., last-writer-wins)
- **AND** version history captures all edits for audit purposes

