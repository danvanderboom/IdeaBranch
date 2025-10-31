## Why
Enable users to view and audit the history of edits to topic nodes, supporting the requirement for version history tracking. This is a prerequisite for sync service implementation, which will need to handle version history synchronization.

## What Changes
- Add `version_history` table to SQLite database schema to store snapshots of topic node changes
- Extend `TopicDb` with migration to create version history table
- Create `IVersionHistoryRepository` interface and `SqliteVersionHistoryRepository` implementation
- Integrate version history capture into `SqliteTopicTreeRepository.SaveAsync` to record changes
- Add domain model `TopicNodeVersion` to represent a historical version
- Add UI page/view to display version history for a topic node
- Add ViewModel for version history display
- Update `TopicNodeDetailPage` to include a "View History" button

## Impact
- Affected specs: data-storage (MODIFIED - implementing existing requirement)
- Affected code: 
  - `src/IdeaBranch.Infrastructure/Storage/TopicDb.cs` (migration)
  - `src/IdeaBranch.Infrastructure/Storage/SqliteTopicTreeRepository.cs` (history capture)
  - `src/IdeaBranch.Domain/` (new `TopicNodeVersion` class)
  - `src/IdeaBranch.Infrastructure/Storage/` (new repository interface and implementation)
  - `src/IdeaBranch.App/Views/` (new `VersionHistoryPage.xaml`)
  - `src/IdeaBranch.App/ViewModels/VersionHistoryViewModel.cs` (new ViewModel)
  - `src/IdeaBranch.App/Views/TopicNodeDetailPage.xaml` (add history button)
  - Tests: integration tests for version history storage and retrieval

