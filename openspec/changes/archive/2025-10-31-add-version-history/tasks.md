## 1. Implementation
- [x] 1.1 Create domain model `TopicNodeVersion` in `src/IdeaBranch.Domain/`
- [x] 1.2 Add migration v2 to `TopicDb` to create `version_history` table
- [x] 1.3 Create `IVersionHistoryRepository` interface
- [x] 1.4 Implement `SqliteVersionHistoryRepository` with Save and GetByNodeId methods
- [x] 1.5 Integrate version history capture into `SqliteTopicTreeRepository.SaveAsync`
- [x] 1.6 Create `VersionHistoryViewModel` in `src/IdeaBranch.App/ViewModels/`
- [x] 1.7 Create `VersionHistoryPage.xaml` and code-behind
- [x] 1.8 Add "View History" button to `TopicNodeDetailPage.xaml`
- [x] 1.9 Register `IVersionHistoryRepository` in `MauiProgram.cs`
- [x] 1.10 Write integration tests for version history storage and retrieval
- [x] 1.11 Write UI tests for version history viewing (optional - defer if needed)

