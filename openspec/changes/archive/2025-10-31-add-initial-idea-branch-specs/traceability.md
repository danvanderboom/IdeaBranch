# Traceability Index: Requirements → Scenarios → Tests

Note: Test IDs prefixed with `IB-UI-` indicate Windows UI automation; others may be covered by integration or build validation steps.

| Capability | Requirement | Scenario | Planned Test ID | Test Type |
| --- | --- | --- | --- | --- |
| ui | Primary navigation exposes core views | Navigate to Topic Tree | IB-UI-011 | UI |
| ui | Primary navigation exposes core views | Navigate to Map | IB-UI-012 | UI |
| ui | Primary navigation exposes core views | Navigate to Timeline | IB-UI-013 | UI |
| ui | App UI exposes stable AutomationIds | Primary navigation AutomationIds exist | IB-UI-010 | UI |
| performance | App responsiveness and startup | Cold start within budget (Windows) | IB-UI-001 | UI |
| product | Hierarchical topic organization | List responses create child nodes | IB-UI-020 | UI |
| settings | Language and regional settings | Change language persists | IB-UI-030 | UI |
| localization | Localizable UI strings | Switch language updates strings | IB-UI-031 | UI |
| localization | Locale-aware formatting | Dates formatted per locale | IB-UI-032 | UI |
| accessibility | Screen reader support | Screen reader announces navigation items | IB-UI-040 | UI |
| accessibility | Keyboard navigation | Navigate primary views via keyboard | IB-UI-041 | UI |
| error-handling | Graceful error handling | Network unavailable | IB-UI-050 | UI |
| error-handling | Graceful error handling | Model/API error | IB-UI-051 | UI |
| telemetry | Feature usage events | Navigation event emitted | IB-UI-060 | UI |
| notifications | In-app notifications | Due date reminder appears | IB-UI-070 | UI |
| notifications | Push notifications with consent | Disable push prevents notifications | IB-UI-071 | UI |
| data-storage | Persist core domain data | Save topic tree changes | IB-UI-080 | UI |
| sync | Offline edits with background sync | Edit offline then sync on reconnect | IB-UI-090 | UI |

Out-of-scope for UI tests (validated elsewhere):

| Capability | Requirement | Scenario | Validation |
| --- | --- | --- | --- |
| platforms | .NET MAUI Windows/iOS support | Windows build succeeds | Build pipeline |
| platforms | .NET MAUI Windows/iOS support | iOS build succeeds | Build pipeline |
| security | Sensitive data protection | Transport security | Static/Integration |
| security | Sensitive data protection | Data at rest protection | Static/Integration |

## Conventions
- Reference scenarios by exact header text in test names or metadata.
- Keep one primary Test ID per scenario; allow additional coverage tests to reference the same ID.
