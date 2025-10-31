## 1. Implementation
- [ ] 1.1 Create domain model `TopicNodeVersion` in `src/IdeaBranch.Domain/`
- [ ] 1.2 Add migration v2 to `TopicDb` to create `version_history` table
- [ ] 1.3 Create `IVersionHistoryRepository` interface
- [ ] 1.4 Implement `SqliteVersionHistoryRepository` with Save and GetByNodeId methods
- [ ] 1.5 Integrate version history capture into `SqliteTopicTreeRepository.SaveAsync`
- [ ] 1.6 Create `VersionHistoryViewModel` in `src/IdeaBranch.App/ViewModels/`
- [ ] 1.7 Create `VersionHistoryPage.xaml` and code-behind
- [ ] 1.8 Add "View History" button to `TopicNodeDetailPage.xaml`
- [ ] 1.9 Register `IVersionHistoryRepository` in `MauiProgram.cs`
- [ ] 1.10 Write integration tests for version history storage and retrieval
- [ ] 1.11 Write UI tests for version history viewing (optional - defer if needed)

