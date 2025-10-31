## Why
We need to evaluate how different hierarchical data presentations affect LLM agent decision-making accuracy. This requires live integration tests that can call Azure OpenAI (or local models) via Microsoft Agent Framework while providing tree-structured context in various formats (expanded/collapsed, depth-limited, property-filtered).

## What Changes
- Add integration test harness for Agent Framework with Azure OpenAI and LM Studio support
- Create scenario-based tests comparing different tree view presentations
- Add PowerShell scripts for easy test execution across configurations
- Document testing approach and environment setup

## Impact
- Affected specs: `agent-interface` (testing methodology)
- Affected code: `CriticalInsight.Data.UnitTests/Agents/Integration/`, `scripts/`
