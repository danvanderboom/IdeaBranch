## Why
Establish standardized patterns for test data generation, database isolation, and UI test cleanup across .NET test projects to improve test maintainability, reduce duplication, and ensure consistent isolation and artifact capture.

## What Changes
- Add test data builder/factory base classes with fluent API for generating valid domain entities
- Provide SQLite database isolation helpers (in-memory per-fixture with transaction-per-test, and temp-file per-test for parallelism)
- Add UI test cleanup patterns with artifact capture on failure and storage reset
- Create shared test infrastructure under `tests/Common/` for cross-project reuse

## Impact
- Affected specs: testing (new capability)
- Affected code: `tests/Common/Factories/`, `tests/Common/Database/`, `tests/UI/`, test projects (`IdeaBranch.UnitTests`, `IdeaBranch.IntegrationTests`, `IdeaBranch.UITests`)

