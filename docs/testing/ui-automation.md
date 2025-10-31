## UI Automation (Windows-first, .NET MAUI UITest/XHarness)

This document outlines how we run UI automation for Idea Branch on Windows using .NET MAUI UITest with XHarness. iOS will follow once Windows flows are stable.

### Goals
- Smoke validation of launch and primary navigation
- Stable `AutomationId` coverage for critical UI
- Runnable locally and in CI

### Prerequisites
- .NET 8 SDK
- Windows 10/11 with Desktop development tools
- Optional: `xharness` CLI for future device automation

Install XHarness CLI (optional for now):
```bash
dotnet tool install --global Microsoft.DotNet.XHarness.CLI
```

### Project layout
- `tests/IdeaBranch.UITests/` — NUnit-based test project scaffold

### Running tests (local)
```bash
dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Debug
```

### CI integration (outline)
1. Restore and build the app (Windows target)
2. Build and run tests:
   ```bash
   dotnet test tests/IdeaBranch.UITests/IdeaBranch.UITests.csproj -c Release
   ```
3. Publish artifacts (test results, logs)

### Conventions
- Every interactive control in critical flows SHALL have a stable `AutomationId`
- Tests SHOULD avoid timing flakiness; prefer deterministic waits or explicit signals

### Next steps
- Wire up MAUI UITest runner for launching the app under test
- Add end-to-end flows per product doc sections

### Scenario → Test ID mapping (Windows UI)

Refer to `openspec/changes/add-initial-idea-branch-specs/traceability.md` for the full matrix. Key items:

| Scenario | Test ID |
| --- | --- |
| Cold start within budget (Windows) | IB-UI-001 |
| Primary navigation AutomationIds exist | IB-UI-010 |
| Navigate to Topic Tree | IB-UI-011 |
| Navigate to Map | IB-UI-012 |
| Navigate to Timeline | IB-UI-013 |
| List responses create child nodes | IB-UI-020 |
| Change language persists | IB-UI-030 |
| Switch language updates strings | IB-UI-031 |
| Dates formatted per locale | IB-UI-032 |
| Screen reader announces navigation items | IB-UI-040 |
| Navigate primary views via keyboard | IB-UI-041 |
| Network unavailable | IB-UI-050 |
| Model/API error | IB-UI-051 |
| Navigation event emitted | IB-UI-060 |
| Due date reminder appears | IB-UI-070 |
| Disable push prevents notifications | IB-UI-071 |
| Save topic tree changes | IB-UI-080 |
| Edit offline then sync on reconnect | IB-UI-090 |

