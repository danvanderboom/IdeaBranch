# Testing Plan: IdeaBranch Requirements

This document outlines how we systematically test our OpenSpec requirements to ensure IdeaBranch meets its specifications.

## Overview

Our testing strategy uses multiple layers:
1. **UI Automation** - End-to-end validation of user-facing scenarios (Windows-first)
2. **Integration Tests** - Service/API interactions, sync, data persistence
3. **Unit Tests** - Component logic, domain rules, edge cases
4. **Build Validation** - Platform targets, security checks, static analysis

## Test Layers by Requirement Type

### UI Requirements (Windows-first with .NET MAUI UITest/XHarness)

**Location**: `tests/IdeaBranch.UITests/`

**Coverage**:
- Navigation flows (IB-UI-011, IB-UI-012, IB-UI-013)
- AutomationId exposure (IB-UI-010)
- Launch performance (IB-UI-001)
- Accessibility (IB-UI-040, IB-UI-041)
- Localization (IB-UI-030, IB-UI-031, IB-UI-032)
- Error handling UI (IB-UI-050, IB-UI-051)
- Notifications (IB-UI-070, IB-UI-071)

**Implementation**:
- Use NUnit with `[Property("TestId", "...")]` annotations
- Launch app via MAUI UITest/XHarness
- Query UI via AutomationId properties
- Assert expected behaviors per scenario
- Use `CriticalInsight.Data.TreeView` via `TopicTreeViewProvider` for hierarchical display testing

**See**: `docs/testing/ui-automation.md` and `openspec/changes/add-initial-idea-branch-specs/traceability.md`

**Note**: TopicTree page uses `CriticalInsight.Data.Hierarchical.TreeView` for hierarchical display:
- `TreeView.ProjectedCollection` bound to `CollectionView` (flattened, filterable projection)
- `Depth` property on `ITreeNode` used for indentation (via `DepthToThicknessConverter`)
- Expand/collapse via `SetIsExpanded()` on `TreeView`
- Test hierarchical scenarios via adapter in `TopicTreeTests.cs`

### Integration Tests

**Requirements covered**:
- Data persistence (Save topic tree changes - IB-UI-080)
- Sync/offline (Edit offline then sync - IB-UI-090)
- Version history (from data-storage capability)
- API/Model interactions (Model/API error - IB-UI-051)

**Location**: `tests/IdeaBranch.IntegrationTests/` (to be created)

**Focus areas**:
- Database/ORM operations
- API client error handling
- Background sync workflows
- Real-time collaboration (future)

### Unit Tests

**Requirements covered**:
- Topic tree manipulation logic
- List-to-child-node conversion
- Tag taxonomy validation
- Prompt template processing
- AI response parsing

**Location**: `tests/IdeaBranch.UnitTests/` (to be created)

**Focus areas**:
- Domain models and business logic
- Parsing/transformation utilities
- Validation rules
- Edge cases and error paths

### Build Validation

**Requirements covered**:
- Windows build succeeds (Build pipeline)
- iOS build succeeds (Build pipeline)
- Security: Transport security (Static/Integration)
- Security: Data at rest protection (Static/Integration)

**Implementation**:
- CI/CD pipeline build steps
- Static analysis tools (e.g., .NET Security Analyzer)
- Platform-specific build validation

## Test Execution Strategy

### Local Development
- Run unit tests frequently (fast feedback)
- Run integration tests before commits
- Run UI tests before PR submission

### CI Pipeline
1. **Build validation** - Ensure all platforms build successfully
2. **Unit tests** - Fast feedback on logic errors
3. **Integration tests** - Validate service interactions
4. **UI automation** - End-to-end validation (Windows first)

### Test Data Management
- Use isolated test databases/storage
- Seed data per test scenario
- Clean up after test runs
- Avoid shared state between tests

## Requirement Verification Matrix

### Performance Requirements
- **Cold start within budget**: Measure app launch time via UI test (IB-UI-001)
- **Responsiveness**: Monitor frame rates, response times during UI automation

### Accessibility Requirements
- **Screen reader support**: Verify AutomationId presence (IB-UI-040)
- **Keyboard navigation**: Test Tab/Enter/Space flows (IB-UI-041)
- **Tool**: Use Windows Narrator or NVDA for screen reader validation

### Localization Requirements
- **Language switching**: UI test to change language and verify strings (IB-UI-030, IB-UI-031)
- **Locale formatting**: Verify date/number formats (IB-UI-032)
- **Test languages**: Start with English; add languages incrementally

### Error Handling Requirements
- **Network unavailable**: Simulate network failure (IB-UI-050)
- **Model/API error**: Mock API error responses (IB-UI-051)
- **Validation**: Ensure user sees actionable error messages

### Data Persistence Requirements
- **Save topic tree changes**: Create/edit node, restart app, verify persistence (IB-UI-080)
- **Version history**: Edit node, verify history is recorded

### Sync Requirements
- **Offline edits**: Edit offline, reconnect, verify sync (IB-UI-090)
- **Conflict handling**: Simulate concurrent edits, verify deterministic result

### Notification Requirements
- **In-app notifications**: Trigger due date, verify notification appears (IB-UI-070)
- **Push notifications**: Toggle setting, verify behavior (IB-UI-071)

## Testing Tools & Frameworks

### UI Testing
- **Framework**: NUnit
- **Runner**: .NET MAUI UITest with XHarness
- **Assertions**: NUnit assertions
- **Test IDs**: Track via `[Property("TestId", "...")]`

### Integration Testing
- **Framework**: NUnit or xUnit (to be decided)
- **Mocking**: Moq or NSubstitute (for API clients)
- **Database**: In-memory or test containers

### Unit Testing
- **Framework**: NUnit or xUnit (to be decided)
- **Mocking**: Moq or NSubstitute
- **Test organization**: One test class per domain class/component

### Static Analysis
- **Security**: .NET Security Analyzer, OWASP dependency checks
- **Code Quality**: SonarAnalyzer, StyleCop (if applicable)

## Test Coverage Goals

### Initial Release (MVP)
- **UI tests**: Cover primary navigation and launch (IB-UI-001, IB-UI-010, IB-UI-011, IB-UI-012, IB-UI-013)
- **Integration tests**: Core data persistence (IB-UI-080)
- **Unit tests**: Critical domain logic (topic tree manipulation)

### Future Iterations
- Expand UI test coverage to all scenarios
- Add accessibility testing automation
- Cover sync and collaboration scenarios
- Add performance benchmarks

## Continuous Improvement

### Test Maintenance
- Keep tests fast (unit <1s, integration <10s, UI <30s per test)
- Refactor tests when implementation changes
- Remove obsolete tests promptly

### Metrics to Track
- Test execution time
- Flaky test rate
- Coverage percentage per capability
- Requirements → test mapping completeness

## Next Steps

1. **Set up unit test project** (`tests/IdeaBranch.UnitTests/`)
2. **Set up integration test project** (`tests/IdeaBranch.IntegrationTests/`)
3. **Implement UI test runner** for MAUI app launch
4. **Create first UI test** (IB-UI-001: Cold start)
5. **Add CI pipeline** with test execution steps
6. **Establish test data patterns**

